using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class ProductImageRepository : IProductImageRepository
{
    private readonly ApplicationDbContext _db;

    public ProductImageRepository(ApplicationDbContext db) => _db = db;

    public Task<ProductImage?> GetByIdAsync(Guid productImageId, CancellationToken cancellationToken)
        => _db.ProductImages.FirstOrDefaultAsync(
            i => i.ImageId == productImageId, cancellationToken);

    public async Task<IReadOnlyList<ProductImage>> GetForProductAsync(
        Guid productId, CancellationToken cancellationToken)
        => await _db.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductImage>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return Array.Empty<ProductImage>();
        }

        return await _db.ProductImages
            .Where(i => productIds.Contains(i.ProductId))
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task ClearPrimaryAsync(Guid productId, CancellationToken cancellationToken)
    {
        var current = await _db.ProductImages
            .Where(i => i.ProductId == productId && i.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var image in current)
        {
            image.IsPrimary = false;
        }
    }

    public async Task<int> NextDisplayOrderAsync(Guid productId, CancellationToken cancellationToken)
    {
        var max = await _db.ProductImages
            .Where(i => i.ProductId == productId)
            .Select(i => (int?)i.DisplayOrder)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }

    public void Add(ProductImage image) => _db.ProductImages.Add(image);

    public void Remove(ProductImage image) => _db.ProductImages.Remove(image);
}