using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Inventory;

public static class InventoryMappings
{
    public static InventoryResponse ToResponse(this Domain.Entities.Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        ProductId = i.ProductId,
        ProductName = i.Product?.ProductName ?? string.Empty,
        Sku = i.Product?.Sku ?? string.Empty,
        WarehouseId = i.WarehouseId,
        WarehouseName = i.Warehouse?.WarehouseName ?? string.Empty,
        StockQuantity = i.StockQuantity,
        ReservedQuantity = i.ReservedQuantity,
        ReorderLevel = i.ReorderLevel,
        DispatchEstimateDays = i.DispatchEstimateDays,
        UpdatedAt = i.UpdatedAt
    };

    public static InventoryWarehouseStockDto ToWarehouseStockDto(this Domain.Entities.Inventory i) => new()
    {
        InventoryId = i.InventoryId,
        WarehouseId = i.WarehouseId,
        WarehouseName = i.Warehouse?.WarehouseName ?? string.Empty,
        StockQuantity = i.StockQuantity,
        ReservedQuantity = i.ReservedQuantity,
        ReorderLevel = i.ReorderLevel,
        DispatchEstimateDays = i.DispatchEstimateDays,
        UpdatedAt = i.UpdatedAt
    };

    public static InventoryMovementResponse ToResponse(this InventoryMovement m) => new()
    {
        MovementId = m.MovementId,
        ProductId = m.ProductId,
        ProductName = m.Product?.ProductName ?? string.Empty,
        Sku = m.Product?.Sku ?? string.Empty,
        WarehouseId = m.WarehouseId,
        WarehouseName = m.Warehouse?.WarehouseName ?? string.Empty,
        Type = m.Type,
        Quantity = m.Quantity,
        Reason = m.Reason,
        OrderId = m.OrderId,
        CreatedByUserId = m.CreatedByUserId,
        CreatedAt = m.CreatedAt
    };
}
