using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.AddToCart;

public sealed class AddToCartCommandHandler : ICommandHandler<AddToCartCommand, Result<CartItemResponse>>
{
    private readonly IProductRepository _products;
    private readonly ICartRepository _carts;
    private readonly IInventoryRepository _inventory;
    private readonly ITaxRuleRepository _taxRules;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartCommandHandler(
        IProductRepository products,
        ICartRepository carts,
        IInventoryRepository inventory,
        ITaxRuleRepository taxRules,
        IUnitOfWork unitOfWork)
    {
        _products = products;
        _carts = carts;
        _inventory = inventory;
        _taxRules = taxRules;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CartItemResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Result.Failure<CartItemResponse>(CartErrors.InvalidQuantity);
        }

        var product = await _products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<CartItemResponse>(CatalogErrors.ProductNotFound);
        }

        if (!string.Equals(product.Status, ProductStatus.Active, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CartItemResponse>(CartErrors.ProductUnavailable);
        }

        var stockRows = await _inventory.GetByProductAsync(request.ProductId, warehouseId: null, cancellationToken);
        var available = stockRows.Sum(i => i.StockQuantity - i.ReservedQuantity);

        var cart = await _carts.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);

        var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == request.ProductId);
        var desiredQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

        if (desiredQuantity > available)
        {
            return Result.Failure<CartItemResponse>(CartErrors.InsufficientStock);
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = desiredQuantity;
        }
        else
        {
            existingItem = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Product = product
            };

            _carts.AddItem(existingItem);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var taxRules = await _taxRules.GetAllAsync(cancellationToken);

        return Result.Success(existingItem.ToResponse(available, taxRules));
    }
}