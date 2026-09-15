using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.CreateWarehouse;

public sealed record CreateWarehouseCommand : ICommand<Result<WarehouseResponse>>
{
    [Required]
    [MaxLength(150)]
    public string WarehouseName { get; init; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; init; }

    public WarehouseStatus Status { get; init; } = WarehouseStatus.Active;
}
