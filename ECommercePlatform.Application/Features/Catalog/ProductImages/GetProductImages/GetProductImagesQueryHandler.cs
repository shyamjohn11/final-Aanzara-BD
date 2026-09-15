using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.ProductImages.GetProductImages;

public sealed class GetProductImagesQueryHandler
    : IQueryHandler<GetProductImagesQuery, Result<IReadOnlyList<ProductImageResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IProductImageRepository _images;

    public GetProductImagesQueryHandler(
        IProductRepository products, IProductImageRepository images)
    {
        _products = products;
        _images = images;
    }

    public async Task<Result<IReadOnlyList<ProductImageResponse>>> Handle(
        GetProductImagesQuery request, CancellationToken cancellationToken)
    {
        if (await _products.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            return Result.Failure<IReadOnlyList<ProductImageResponse>>(
                CatalogErrors.ProductNotFound);
        }

        var images = await _images.GetForProductAsync(request.ProductId, cancellationToken);

        return Result.Success<IReadOnlyList<ProductImageResponse>>(
            images.OrderBy(i => i.DisplayOrder).Select(i => i.ToResponse()).ToArray());
    }
}
