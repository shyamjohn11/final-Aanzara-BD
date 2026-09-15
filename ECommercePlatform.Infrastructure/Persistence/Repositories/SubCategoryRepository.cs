using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class SubCategoryRepository : ISubCategoryRepository
{
    private readonly ApplicationDbContext _db;

    public SubCategoryRepository(ApplicationDbContext db) => _db = db;

    public Task<SubCategory?> GetByIdAsync(Guid subCategoryId, CancellationToken cancellationToken)
        => _db.SubCategories.FirstOrDefaultAsync(
            s => s.SubCategoryId == subCategoryId, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken)
        => _db.SubCategories.AnyAsync(
            s => s.SubCategoryCode == code && (excludingId == null || s.SubCategoryId != excludingId),
            cancellationToken);

    public Task<bool> HasProductsAsync(Guid subCategoryId, CancellationToken cancellationToken)
        => _db.Products.AnyAsync(p => p.SubCategoryId == subCategoryId, cancellationToken);

    public async Task<PagedResult<SubCategory>> SearchAsync(
        Guid? categoryId, string? search, bool? isActive, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _db.SubCategories.AsNoTracking().AsQueryable();

        if (categoryId is not null)
        {
            query = query.Where(s => s.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                EF.Functions.Like(s.SubCategoryName, $"%{search}%")
                || EF.Functions.Like(s.SubCategoryCode, $"%{search}%"));
        }

        if (isActive is not null)
        {
            query = query.Where(s => s.IsActive == isActive);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.SubCategoryName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SubCategory>(items, page, pageSize, total);
    }

    public void Add(SubCategory subCategory) => _db.SubCategories.Add(subCategory);

    public void Remove(SubCategory subCategory) => _db.SubCategories.Remove(subCategory);
}
