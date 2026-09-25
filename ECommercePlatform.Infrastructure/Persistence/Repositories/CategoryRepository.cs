using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private static readonly TimeSpan TreeCacheDuration = TimeSpan.FromMinutes(5);
    private const string TreeCacheKey = "category-tree:{0}";

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public CategoryRepository(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

    public Task<Category?> GetWithImagesAsync(Guid categoryId, CancellationToken cancellationToken)
        => _db.Categories
            .Include(c => c.Images)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

    public Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken)
        => _db.Categories.AnyAsync(c => c.CategoryId == categoryId, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken)
        => _db.Categories.AnyAsync(
            c => c.CategoryCode == code && (excludingId == null || c.CategoryId != excludingId),
            cancellationToken);

    public async Task<PagedResult<Category>> SearchAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                EF.Functions.Like(c.CategoryName, $"%{search}%")
                || EF.Functions.Like(c.CategoryCode, $"%{search}%"));
        }

        if (isActive is not null)
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        // Count before paging so the client knows how many pages exist.
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.CategoryName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesWithSubCategoriesAsync(CancellationToken cancellationToken)
        => await _db.Categories
            .AsNoTracking()
            .Where(c => c.HasSubCategory && c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CategoryTreeNode>> GetTreeAsync(
        bool activeOnly, CancellationToken cancellationToken)
    {
        var cacheKey = string.Format(TreeCacheKey, activeOnly);
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CategoryTreeNode>? cached) && cached is not null)
        {
            return cached;
        }

        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        // One round trip for the whole tree. Projecting the children inline lets
        // EF translate the product counts into correlated sub-queries instead of
        // us issuing a query per category.
        var tree = await query
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryTreeNode(
                c.CategoryId,
                c.CategoryCode,
                c.CategoryName,
                c.IsActive,
                c.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                c.SubCategories
                    .Where(s => !activeOnly || s.IsActive)
                    .OrderBy(s => s.SubCategoryName)
                    .Select(s => new SubCategoryTreeNode(
                        s.SubCategoryId,
                        s.SubCategoryCode,
                        s.SubCategoryName,
                        s.IsActive,
                        // Sub-category images live in their own table with no
                        // navigation from SubCategory, so correlate directly.
                        _db.SubCategoryImages
                            .Where(i => i.SubCategoryId == s.SubCategoryId && i.IsPrimary)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault(),
                        s.Products.Count(p => !activeOnly || p.Status == ProductStatus.Active)))
                    .ToList()))
            .ToListAsync(cancellationToken);

        IReadOnlyList<CategoryTreeNode> result = tree;
        _cache.Set(cacheKey, result, TreeCacheDuration);
        return result;
    }

    public async Task RefreshHasSubCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);

        if (category is null)
        {
            return;
        }

        // Recomputed from the rows that exist rather than incremented, so the flag
        // cannot drift out of step with reality.
        category.HasSubCategory = await _db.SubCategories
            .AnyAsync(s => s.CategoryId == categoryId, cancellationToken);
    }

    public void Add(Category category) => _db.Categories.Add(category);

    public void Remove(Category category) => _db.Categories.Remove(category);
}
