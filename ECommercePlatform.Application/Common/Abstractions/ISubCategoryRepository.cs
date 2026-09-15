using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ISubCategoryRepository
{
    Task<SubCategory?> GetByIdAsync(Guid subCategoryId, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);

    Task<bool> HasProductsAsync(Guid subCategoryId, CancellationToken cancellationToken);

    Task<PagedResult<SubCategory>> SearchAsync(
        Guid? categoryId, string? search, bool? isActive, int page, int pageSize,
        CancellationToken cancellationToken);

    void Add(SubCategory subCategory);

    void Remove(SubCategory subCategory);
}
