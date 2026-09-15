using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.RemoveWishlistItem;

public sealed class RemoveWishlistItemCommandHandler : ICommandHandler<RemoveWishlistItemCommand, Result>
{
    private readonly IWishlistRepository _wishlists;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWishlistItemCommandHandler(IWishlistRepository wishlists, IUnitOfWork unitOfWork)
    {
        _wishlists = wishlists;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveWishlistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _wishlists.GetByUserAndProductAsync(
            request.UserId, request.ProductId, cancellationToken);

        if (item is null)
        {
            return Result.Failure(WishlistErrors.ItemNotFound);
        }

        _wishlists.Remove(item);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}