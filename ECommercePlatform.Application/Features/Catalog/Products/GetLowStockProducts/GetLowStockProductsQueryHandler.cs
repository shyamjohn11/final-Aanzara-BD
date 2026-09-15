using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetLowStockProducts;

public sealed class GetLowStockProductsQueryHandler
    : IQueryHandler<GetLowStockProductsQuery, Result<IReadOnlyList<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetLowStockProductsQueryHandler(
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
        GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Count <= 0)
        {
            return Result.Failure<IReadOnlyList<ProductSummaryResponse>>(
                CatalogErrors.InvalidProductCount);
        }

        // Same low-stock rule the admin inventory screen uses (quantity at or below
        // reorder level), aggregated across warehouses per product. Fully depleted
        // products are excluded — those belong to no customer-facing shelf — and so
        // are Inactive ones. The page cap truncates rather than scans forever; fine
        // at catalog scale.
        var rows = await _inventory.SearchLowStockAsync(
            warehouseId: null, page: 1, pageSize: 200, cancellationToken);

        var lowStock = rows.Items
            .GroupBy(i => i.ProductId)
            .Select(g => (
                product: g.First().Product,
                available: g.Sum(i => i.StockQuantity - i.ReservedQuantity)))
            .Where(x => x.product.Status == ProductStatus.Active && x.available > 0)
            .OrderBy(x => x.available)
            .Take(request.Count)
            .Select(x => x.product)
            .ToList();

        return Result.Success<IReadOnlyList<ProductSummaryResponse>>(
            await lowStock.ToEnrichedSummariesAsync(_brands, _productImages, _inventory, cancellationToken));
    }
}
