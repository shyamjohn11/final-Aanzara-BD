using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Warehouses;

public static class WarehouseMappings
{
    public static WarehouseResponse ToResponse(this Warehouse w) => new()
    {
        WarehouseId = w.WarehouseId,
        WarehouseName = w.WarehouseName,
        Address = w.Address,
        Status = w.Status,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };
}
