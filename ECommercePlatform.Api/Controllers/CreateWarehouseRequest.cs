using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Api.Controllers;

public sealed class CreateWarehouseRequest
{
    [Required]
    [MaxLength(150)]
    public string WarehouseName { get; init; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; init; }

    public WarehouseStatus Status { get; init; } = WarehouseStatus.Active;
}
