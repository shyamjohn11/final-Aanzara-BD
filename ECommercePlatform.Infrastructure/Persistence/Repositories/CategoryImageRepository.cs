using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class CategoryImageRepository : ICategoryImageRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryImageRepository(ApplicationDbContext db) => _db = db;

    public Task<CategoryImage?> GetByIdAsync(Guid categoryImageId, CancellationToken cancellationToken)
        => _db.CategoryImages.FirstOrDefaultAsync(
            i => i.CategoryImageId == categoryImageId, cancellationToken);

    public async Task<IReadOnlyList<CategoryImage>> GetForCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken)
        => await _db.CategoryImages
            .Where(i => i.CategoryId == categoryId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CategoryImage>> GetForCategoriesAsync(
        IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken)
        => await _db.CategoryImages
            .Where(i => categoryIds.Contains(i.CategoryId))
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task ClearPrimaryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        // Loaded rather than bulk-updated because the caller is mid-transaction and
        // the change tracker must see this, or a filtered unique index violation
        // would surface on commit.
        var current = await _db.CategoryImages
            .Where(i => i.CategoryId == categoryId && i.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var image in current)
        {
            image.IsPrimary = false;
        }
    }

    public async Task<int> NextDisplayOrderAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var max = await _db.CategoryImages
            .Where(i => i.CategoryId == categoryId)
            .Select(i => (int?)i.DisplayOrder)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }

    public void Add(CategoryImage image) => _db.CategoryImages.Add(image);

    public void Remove(CategoryImage image) => _db.CategoryImages.Remove(image);
}
