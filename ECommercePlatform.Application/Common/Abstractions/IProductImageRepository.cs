using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IProductImageRepository
{
    Task<ProductImage?> GetByIdAsync(Guid productImageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductImage>> GetForProductAsync(
        Guid productId, CancellationToken cancellationToken);

    /// <summary>
    /// Every image for the given products in one query, so list endpoints can pick
    /// a primary image per product without N+1 lookups.
    /// </summary>
    Task<IReadOnlyList<ProductImage>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    Task ClearPrimaryAsync(Guid productId, CancellationToken cancellationToken);

    Task<int> NextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken);

    void Add(ProductImage image);

    void Remove(ProductImage image);
}