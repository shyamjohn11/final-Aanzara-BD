using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ISubCategoryImageRepository
{
    Task<SubCategoryImage?> GetByIdAsync(Guid imageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SubCategoryImage>> GetForSubCategoryAsync(
        Guid subCategoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SubCategoryImage>> GetForSubCategoriesAsync(
        IReadOnlyCollection<Guid> subCategoryIds, CancellationToken cancellationToken);

    void Add(SubCategoryImage image);

    void Remove(SubCategoryImage image);
}