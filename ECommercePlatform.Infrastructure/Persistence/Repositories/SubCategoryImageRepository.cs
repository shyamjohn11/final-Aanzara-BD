using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class SubCategoryImageRepository : ISubCategoryImageRepository
{
    private readonly ApplicationDbContext _db;

    public SubCategoryImageRepository(ApplicationDbContext db) => _db = db;

    public Task<SubCategoryImage?> GetByIdAsync(Guid imageId, CancellationToken cancellationToken)
        => _db.SubCategoryImages.FirstOrDefaultAsync(i => i.ImageId == imageId, cancellationToken);

    public async Task<IReadOnlyList<SubCategoryImage>> GetForSubCategoryAsync(
        Guid subCategoryId, CancellationToken cancellationToken)
        => await _db.SubCategoryImages
            .Where(i => i.SubCategoryId == subCategoryId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubCategoryImage>> GetForSubCategoriesAsync(
        IReadOnlyCollection<Guid> subCategoryIds, CancellationToken cancellationToken)
        => await _db.SubCategoryImages
            .Where(i => subCategoryIds.Contains(i.SubCategoryId))
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public void Add(SubCategoryImage image) => _db.SubCategoryImages.Add(image);

    public void Remove(SubCategoryImage image) => _db.SubCategoryImages.Remove(image);
}