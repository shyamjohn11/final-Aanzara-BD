using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetWarehouses;

public sealed record GetWarehousesQuery : IQuery<Result<PagedResult<WarehouseResponse>>>
{
    [MaxLength(200)]
    public string? Search { get; init; }

    public WarehouseStatus? Status { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}
