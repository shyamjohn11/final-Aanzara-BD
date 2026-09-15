using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Wishlist.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.GetMyWishlist;

public sealed class GetMyWishlistQueryHandler
    : IQueryHandler<GetMyWishlistQuery, Result<IReadOnlyList<WishlistItemResponse>>>
{
    private readonly IWishlistRepository _wishlists;

    public GetMyWishlistQueryHandler(IWishlistRepository wishlists) => _wishlists = wishlists;

    public async Task<Result<IReadOnlyList<WishlistItemResponse>>> Handle(
        GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        var items = await _wishlists.GetByUserIdAsync(request.UserId, cancellationToken);

        var response = items
            .Select(w => new WishlistItemResponse(
                w.WishlistItemId,
                w.ProductId,
                w.Product.ProductName,
                w.Product.Sku,
                w.Product.Price,
                w.Product.Mrp,
                w.Product.Status))
            .ToArray();

        return Result.Success<IReadOnlyList<WishlistItemResponse>>(response);
    }
}