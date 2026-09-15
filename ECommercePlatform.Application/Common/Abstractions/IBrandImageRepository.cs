using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IBrandImageRepository
{
    Task<BrandImage?> GetByIdAsync(Guid brandImageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BrandImage>> GetForBrandAsync(
        Guid brandId, CancellationToken cancellationToken);

    void Add(BrandImage image);

    void Remove(BrandImage image);
}