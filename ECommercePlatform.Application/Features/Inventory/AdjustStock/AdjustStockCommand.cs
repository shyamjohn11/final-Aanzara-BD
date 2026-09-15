using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.AdjustStock;

public sealed record AdjustStockCommand : ICommand<Result<InventoryResponse>>
{
    public Guid ProductId { get; init; }

    [Required]
    public Guid WarehouseId { get; init; }

    /// <summary>
    /// Quantity adjustment: positive increases stock, negative decreases stock.
    /// </summary>
    public int Quantity { get; init; }

    [MaxLength(255)]
    public string? Reason { get; init; }
}
