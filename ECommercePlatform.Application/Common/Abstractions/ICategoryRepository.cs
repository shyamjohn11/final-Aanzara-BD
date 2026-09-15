using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Loads the category with its images, for the detail view.</summary>
    Task<Category?> GetWithImagesAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken);

    /// <summary>Codes are unique. Pass <paramref name="excludingId"/> when checking during an update.</summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);

    Task<PagedResult<Category>> SearchAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Lightweight list of categories that have sub-categories.</summary>
    Task<IReadOnlyList<Category>> GetCategoriesWithSubCategoriesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The whole product tree in one round trip: categories, their sub-categories,
    /// and each sub-category's product count.
    /// </summary>
    Task<IReadOnlyList<CategoryTreeNode>> GetTreeAsync(
        bool activeOnly, CancellationToken cancellationToken);

    /// <summary>Recomputes HasSubCategory from the child rows that actually exist.</summary>
    Task RefreshHasSubCategoryAsync(Guid categoryId, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);
}

/// <summary>Flattened tree row, shaped by the query rather than assembled in memory.</summary>
public sealed record CategoryTreeNode(
    Guid CategoryId,
    string CategoryCode,
    string CategoryName,
    bool IsActive,
    string? PrimaryImageUrl,
    IReadOnlyList<SubCategoryTreeNode> SubCategories);

public sealed record SubCategoryTreeNode(
    Guid SubCategoryId,
    string SubCategoryCode,
    string SubCategoryName,
    bool IsActive,
    string? PrimaryImageUrl,
    int ProductCount);
