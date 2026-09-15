using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetInventoryMovements;

public sealed record GetInventoryMovementsQuery : IQuery<Result<PagedResult<InventoryMovementResponse>>>
{
    public Guid ProductId { get; init; }

    public Guid? WarehouseId { get; init; }

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}
