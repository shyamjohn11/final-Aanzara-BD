using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.RemoveCartItem;

/// <summary>
/// Removes one item from the caller's cart. Ownership is enforced by the
/// repository lookup, which joins through Cart.UserId — an id belonging to
/// another user's cart is simply not found.
/// </summary>
public sealed class RemoveCartItemCommandHandler
    : ICommandHandler<RemoveCartItemCommand, Result>
{
    private readonly ICartRepository _carts;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCartItemCommandHandler(ICartRepository carts, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _carts.GetItemByIdAndUserIdAsync(
            request.CartItemId, request.UserId, cancellationToken);

        if (item is null)
        {
            return Result.Failure(CartErrors.ItemNotFound);
        }

        _carts.RemoveItem(item);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
