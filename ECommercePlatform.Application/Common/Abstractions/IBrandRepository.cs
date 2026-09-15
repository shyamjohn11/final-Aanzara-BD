using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid brandId, CancellationToken cancellationToken);

    /// <summary>Batch lookup for list endpoints that render a brand name per row.</summary>
    Task<IReadOnlyCollection<Brand>> GetByIdsAsync(
        IReadOnlyCollection<Guid> brandIds, CancellationToken cancellationToken);

    /// <summary>Loads the brand with its images, for detail/update/delete.</summary>
    Task<Brand?> GetWithImagesAsync(Guid brandId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid brandId, CancellationToken cancellationToken);

    /// <summary>
    /// Names are treated as unique. Pass <paramref name="excludingId"/> when checking during an update.
    /// </summary>
    Task<bool> NameExistsAsync(
        string brandName,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task<bool> HasProductsAsync(Guid brandId, CancellationToken cancellationToken);

    Task<PagedResult<Brand>> SearchAsync(
        string? search,
        BrandStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(Brand brand);

    void Remove(Brand brand);
}