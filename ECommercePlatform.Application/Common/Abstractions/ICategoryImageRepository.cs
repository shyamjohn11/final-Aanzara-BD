using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ICategoryImageRepository
{
    Task<CategoryImage?> GetByIdAsync(Guid categoryImageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryImage>> GetForCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryImage>> GetForCategoriesAsync(
        IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken);

    /// <summary>
    /// Clears the primary flag across a category's images so a new primary can be
    /// set without two rows claiming it.
    /// </summary>
    Task ClearPrimaryAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<int> NextDisplayOrderAsync(Guid categoryId, CancellationToken cancellationToken);

    void Add(CategoryImage image);

    void Remove(CategoryImage image);
}
