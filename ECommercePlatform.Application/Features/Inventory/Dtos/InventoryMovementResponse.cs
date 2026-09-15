using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Application.Features.Inventory.Dtos;

public sealed record InventoryMovementResponse
{
    public Guid MovementId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public InventoryMovementType Type { get; init; }
    public int Quantity { get; init; }
    public string? Reason { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
