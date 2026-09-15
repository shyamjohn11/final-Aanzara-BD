using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetInventoryList;

public sealed record GetInventoryListQuery : IQuery<Result<PagedResult<InventoryResponse>>>
{
    public Guid? WarehouseId { get; init; }

    public Guid? ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}
