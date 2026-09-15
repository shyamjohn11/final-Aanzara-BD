using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.ReceiveStock;

public sealed record ReceiveStockCommand : ICommand<Result<InventoryResponse>>
{
    public Guid ProductId { get; init; }

    [Required]
    public Guid WarehouseId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    public InventoryMovementType Type { get; init; } = InventoryMovementType.IN;

    [MaxLength(255)]
    public string? Reason { get; init; }
}
