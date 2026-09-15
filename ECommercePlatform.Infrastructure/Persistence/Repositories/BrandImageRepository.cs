using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class BrandImageRepository : IBrandImageRepository
{
    private readonly ApplicationDbContext _db;

    public BrandImageRepository(ApplicationDbContext db) => _db = db;

    public Task<BrandImage?> GetByIdAsync(Guid brandImageId, CancellationToken cancellationToken)
        => _db.BrandImages.FirstOrDefaultAsync(i => i.BrandImageId == brandImageId, cancellationToken);

    public async Task<IReadOnlyList<BrandImage>> GetForBrandAsync(
        Guid brandId, CancellationToken cancellationToken)
        => await _db.BrandImages
            .Where(i => i.BrandId == brandId)
            .ToListAsync(cancellationToken);

    public void Add(BrandImage image) => _db.BrandImages.Add(image);

    public void Remove(BrandImage image) => _db.BrandImages.Remove(image);
}