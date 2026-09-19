namespace ECommercePlatform.Application.Features.Inventory.Dtos;

public sealed record InventoryResponse
{
    public Guid InventoryId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string Category { get; init; } = "Uncategorized";
    public string Brand { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public int ReorderLevel { get; init; }
    public int DispatchEstimateDays { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
