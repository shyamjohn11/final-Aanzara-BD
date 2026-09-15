using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class BrandRepository : IBrandRepository
{
    private readonly ApplicationDbContext _db;

    public BrandRepository(ApplicationDbContext db) => _db = db;

    public Task<Brand?> GetByIdAsync(Guid brandId, CancellationToken cancellationToken)
        => _db.Brands.FirstOrDefaultAsync(b => b.BrandId == brandId, cancellationToken);

    public async Task<IReadOnlyCollection<Brand>> GetByIdsAsync(
        IReadOnlyCollection<Guid> brandIds, CancellationToken cancellationToken)
    {
        if (brandIds.Count == 0)
        {
            return Array.Empty<Brand>();
        }

        return await _db.Brands
            .Where(b => brandIds.Contains(b.BrandId))
            .ToListAsync(cancellationToken);
    }

    public Task<Brand?> GetWithImagesAsync(Guid brandId, CancellationToken cancellationToken)
        => _db.Brands
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.BrandId == brandId, cancellationToken);

    public Task<bool> ExistsAsync(Guid brandId, CancellationToken cancellationToken)
        => _db.Brands.AnyAsync(b => b.BrandId == brandId, cancellationToken);

    public Task<bool> NameExistsAsync(string brandName, Guid? excludingId, CancellationToken cancellationToken)
        => _db.Brands.AnyAsync(
            b => b.BrandName == brandName && (excludingId == null || b.BrandId != excludingId),
            cancellationToken);

    public Task<bool> HasProductsAsync(Guid brandId, CancellationToken cancellationToken)
        => _db.Products.AnyAsync(p => p.BrandId == brandId, cancellationToken);

    public async Task<PagedResult<Brand>> SearchAsync(
        string? search, BrandStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Brands.AsNoTracking().Include(b => b.Images).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b => EF.Functions.Like(b.BrandName, $"%{search}%"));
        }

        if (status is not null)
        {
            query = query.Where(b => b.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.BrandName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Brand>(items, page, pageSize, total);
    }

    public void Add(Brand brand) => _db.Brands.Add(brand);

    public void Remove(Brand brand) => _db.Brands.Remove(brand);
}