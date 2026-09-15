using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Api.Controllers;

public sealed class AdjustStockRequest
{
    [Required]
    public Guid WarehouseId { get; init; }

    /// <summary>
    /// Quantity change: positive to increase, negative to decrease stock.
    /// </summary>
    public int Quantity { get; init; }

    [MaxLength(255)]
    public string? Reason { get; init; }
}
