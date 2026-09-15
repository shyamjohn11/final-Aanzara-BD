using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(
        string warehouseName,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task<bool> HasInventoryAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<PagedResult<Warehouse>> SearchAsync(
        string? search,
        WarehouseStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(Warehouse warehouse);

    void Remove(Warehouse warehouse);
}
