using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetPopularProducts;

public sealed class GetPopularProductsQueryHandler
    : IQueryHandler<GetPopularProductsQuery, Result<IReadOnlyList<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetPopularProductsQueryHandler(
        IProductRepository products,
        IBrandRepository brands,
        IProductImageRepository productImages,
        IInventoryRepository inventory)
    {
        _products = products;
        _brands = brands;
        _productImages = productImages;
        _inventory = inventory;
    }

    public async Task<Result<IReadOnlyList<ProductSummaryResponse>>> Handle(
        GetPopularProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Count <= 0)
        {
            return Result.Failure<IReadOnlyList<ProductSummaryResponse>>(
                CatalogErrors.InvalidProductCount);
        }

        // No sales history is tracked yet, so "popular" is deepest discount first,
        // newest as tie-breaker. Every active product therefore has a stable,
        // non-empty place in this collection.
        var filter = new ProductFilter(
            Status: ProductStatus.Active,
            SortBy: "discount",
            SortDescending: true);

        var page = await _products.SearchAsync(filter, page: 1, pageSize: request.Count, cancellationToken);

        return Result.Success<IReadOnlyList<ProductSummaryResponse>>(
            await page.Items.ToEnrichedSummariesAsync(_brands, _productImages, _inventory, cancellationToken));
    }
}
