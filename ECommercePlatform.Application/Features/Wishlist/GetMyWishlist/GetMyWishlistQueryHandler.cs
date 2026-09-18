using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Wishlist.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Wishlist.GetMyWishlist;

public sealed class GetMyWishlistQueryHandler
    : IQueryHandler<GetMyWishlistQuery, Result<IReadOnlyList<WishlistItemResponse>>>
{
    private readonly IWishlistRepository _wishlists;
    private readonly IProductImageRepository _productImages;

    public GetMyWishlistQueryHandler(
        IWishlistRepository wishlists,
        IProductImageRepository productImages)
    {
        _wishlists = wishlists;
        _productImages = productImages;
    }

    public async Task<Result<IReadOnlyList<WishlistItemResponse>>> Handle(
        GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        var items = await _wishlists.GetByUserIdAsync(request.UserId, cancellationToken);

        // Primary image first, then lowest display order — same rule as
        // ProductSummaryEnrichment, one batch query instead of N+1.
        var primaryImageByProduct = (await _productImages.GetByProductIdsAsync(
                items.Select(w => w.ProductId).Distinct().ToArray(),
                cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .First());

        var response = items
            .Select(w => new WishlistItemResponse(
                w.WishlistItemId,
                w.ProductId,
                w.Product.ProductName,
                w.Product.Sku,
                w.Product.Price,
                w.Product.Mrp,
                w.Product.Status,
                primaryImageByProduct.TryGetValue(w.ProductId, out var image)
                    ? $"/api/v1/products/{w.ProductId}/images/{image.ImageId}/file"
                    : string.Empty))
            .ToArray();

        return Result.Success<IReadOnlyList<WishlistItemResponse>>(response);
    }
}