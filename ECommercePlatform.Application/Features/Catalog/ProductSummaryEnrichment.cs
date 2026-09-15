using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Catalog;

/// <summary>
/// Fills the display-only fields of ProductSummaryResponse (brand name, primary
/// image, stock availability) with one batch lookup each — three queries per
/// page, never three per product. Brand↔Product has no configured EF navigation,
/// so enrichment happens here rather than through Includes.
/// </summary>
public static class ProductSummaryEnrichment
{
    public static async Task<IReadOnlyList<ProductSummaryResponse>> ToEnrichedSummariesAsync(
        this IReadOnlyCollection<Product> products,
        IBrandRepository brands,
        IProductImageRepository productImages,
        IInventoryRepository inventory,
        CancellationToken cancellationToken)
    {
        if (products.Count == 0)
        {
            return Array.Empty<ProductSummaryResponse>();
        }

        var productIds = products.Select(p => p.ProductId).ToArray();
        var brandIds = products
            .Where(p => p.BrandId is not null)
            .Select(p => p.BrandId!.Value)
            .Distinct()
            .ToArray();

        var brandNames = brandIds.Length == 0
            ? new Dictionary<Guid, string>()
            : (await brands.GetByIdsAsync(brandIds, cancellationToken))
                .ToDictionary(b => b.BrandId, b => b.BrandName);

        // Primary image first, then lowest display order as the fallback.
        var primaryImage = (await productImages.GetByProductIdsAsync(productIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .First());

        var stock = (await inventory.GetByProductIdsAsync(productIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => (
                    available: g.Sum(i => i.StockQuantity - i.ReservedQuantity),
                    reorderLevel: g.Max(i => i.ReorderLevel)));

        return products.Select(p =>
        {
            var summary = p.ToSummary();

            if (p.BrandId is not null && brandNames.TryGetValue(p.BrandId.Value, out var brandName))
            {
                summary = summary with { BrandName = brandName };
            }

            if (primaryImage.TryGetValue(p.ProductId, out var image))
            {
                summary = summary with
                {
                    ImageUrl = $"/api/v1/products/{p.ProductId}/images/{image.ImageId}/file"
                };
            }

            if (stock.TryGetValue(p.ProductId, out var s))
            {
                summary = summary with
                {
                    AvailableStock = s.available,
                    StockStatus = s.available <= 0
                        ? "out_of_stock"
                        : s.available <= s.reorderLevel
                            ? "low_stock"
                            : "in_stock"
                };
            }
            // No inventory rows: AvailableStock/StockStatus stay null. Callers treat
            // that as in stock so freshly added products surface immediately.

            return summary;
        }).ToArray();
    }
}
