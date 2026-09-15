using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetInStockProducts;

public sealed class GetInStockProductsQueryHandler
    : IQueryHandler<GetInStockProductsQuery, Result<IReadOnlyList<ProductSummaryResponse>>>
{
    private readonly IProductRepository _products;
    private readonly IBrandRepository _brands;
    private readonly IProductImageRepository _productImages;
    private readonly IInventoryRepository _inventory;

    public GetInStockProductsQueryHandler(
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
        GetInStockProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Count <= 0)
        {
            return Result.Failure<IReadOnlyList<ProductSummaryResponse>>(
                CatalogErrors.InvalidProductCount);
        }

        // Newest active products first; a product counts as in stock when it has
        // no inventory rows at all (untracked — freshly added products surface here
        // immediately) or its available quantity exceeds the reorder level.
        var filter = new ProductFilter(
            Status: ProductStatus.Active,
            SortBy: "created",
            SortDescending: true);

        var page = await _products.SearchAsync(filter, page: 1, pageSize: 500, cancellationToken);

        var candidates = page.Items.ToList();

        var stock = (await _inventory.GetByProductIdsAsync(
                candidates.Select(p => p.ProductId).ToArray(), cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => (
                    available: g.Sum(i => i.StockQuantity - i.ReservedQuantity),
                    reorderLevel: g.Max(i => i.ReorderLevel)));

        var inStock = candidates
            .Where(p => !stock.TryGetValue(p.ProductId, out var s) || s.available > s.reorderLevel)
            .Take(request.Count)
            .ToList();

        return Result.Success<IReadOnlyList<ProductSummaryResponse>>(
            await inStock.ToEnrichedSummariesAsync(_brands, _productImages, _inventory, cancellationToken));
    }
}
