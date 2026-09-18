using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>Filters for the product list endpoint, kept as one value so the signature stays readable.</summary>
public sealed record ProductFilter(
    Guid? CategoryId = null,
    Guid? SubCategoryId = null,
    Guid? BrandId = null,
    Guid? DealerId = null,
    string? Search = null,
    string? Status = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    bool SortDescending = false);

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken);

    Task<int> CountByDealerAsync(Guid dealerId, CancellationToken cancellationToken);

    Task<PagedResult<Product>> SearchAsync(
        ProductFilter filter, int page, int pageSize, CancellationToken cancellationToken);

    void Add(Product product);

    void Remove(Product product);
}