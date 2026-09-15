using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Api.Controllers;

public sealed class ReceiveStockRequest
{
    [Required]
    public Guid WarehouseId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    public InventoryMovementType Type { get; init; } = InventoryMovementType.IN;

    [MaxLength(255)]
    public string? Reason { get; init; }
}
