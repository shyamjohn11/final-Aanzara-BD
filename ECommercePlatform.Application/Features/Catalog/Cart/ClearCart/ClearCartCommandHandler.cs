using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.ClearCart;

/// <summary>
/// Empties the caller's cart, keeping the cart row itself. A caller with no
/// cart yet is already clear, so that is a success — same stance GetCart takes.
/// </summary>
public sealed class ClearCartCommandHandler
    : ICommandHandler<ClearCartCommand, Result>
{
    private readonly ICartRepository _carts;
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartCommandHandler(ICartRepository carts, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart is null)
        {
            return Result.Success();
        }

        foreach (var item in cart.CartItems)
        {
            _carts.RemoveItem(item);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
