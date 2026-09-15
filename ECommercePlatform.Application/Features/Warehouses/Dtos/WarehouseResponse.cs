using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Application.Features.Warehouses.Dtos;

public sealed record WarehouseResponse
{
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? Address { get; init; }
    public WarehouseStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
