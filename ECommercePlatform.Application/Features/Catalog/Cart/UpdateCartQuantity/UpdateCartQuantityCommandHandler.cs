using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.UpdateCartQuantity;
using ECommercePlatform.Domain.Entities;
public sealed class UpdateCartQuantityCommandHandler
    : ICommandHandler<UpdateCartQuantityCommand, Result<CartItemResponse>>
{
    private readonly ICartRepository _carts;
    private readonly IInventoryRepository _inventory;
    private readonly ITaxRuleRepository _taxRules;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartQuantityCommandHandler(
        ICartRepository carts, IInventoryRepository inventory,
        ITaxRuleRepository taxRules, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _inventory = inventory;
        _taxRules = taxRules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartItemResponse>> Handle(
        UpdateCartQuantityCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Result.Failure<CartItemResponse>(CartErrors.InvalidQuantity);
        }

        var item = await _carts.GetItemByIdAndUserIdAsync(request.CartItemId, request.UserId, cancellationToken);

        if (item is null)
        {
            return Result.Failure<CartItemResponse>(CartErrors.ItemNotFound);
        }

        if (!string.Equals(item.Product.Status, ProductStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CartItemResponse>(CartErrors.ProductUnavailable);
        }

        var stockRows = await _inventory.GetByProductAsync(item.ProductId, warehouseId: null, cancellationToken);

        // Same rule as add-to-cart and the shelf: products with no inventory
        // rows are sellable without a cap; only tracked stock is enforced.
        var hasStockRows = stockRows.Count > 0;
        var available = hasStockRows
            ? stockRows.Sum(i => i.StockQuantity - i.ReservedQuantity)
            : int.MaxValue;

        if (hasStockRows && request.Quantity > available)
        {
            return Result.Failure<CartItemResponse>(CartErrors.InsufficientStock);
        }

        item.Quantity = request.Quantity;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var taxRules = await _taxRules.GetAllAsync(cancellationToken);

        return Result.Success(item.ToResponse(available, taxRules));
    }
}