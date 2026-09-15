using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetFreshArrivals;

public sealed class GetFreshArrivalsQueryHandler
    : IQueryHandler<GetFreshArrivalsQuery, Result<IReadOnlyList<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetFreshArrivalsQueryHandler(
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
        GetFreshArrivalsQuery request, CancellationToken cancellationToken)
    {
        if (request.Count <= 0)
        {
            return Result.Failure<IReadOnlyList<ProductSummaryResponse>>(
                CatalogErrors.InvalidFreshArrivalsCount);
        }

        var filter = new ProductFilter(
            Status: ProductStatus.Active,
            SortBy: "created",
            SortDescending: true);

        var page = await _products.SearchAsync(filter, page: 1, pageSize: request.Count, cancellationToken);

        return Result.Success<IReadOnlyList<ProductSummaryResponse>>(
            await page.Items.ToEnrichedSummariesAsync(_brands, _productImages, _inventory, cancellationToken));
    }
}