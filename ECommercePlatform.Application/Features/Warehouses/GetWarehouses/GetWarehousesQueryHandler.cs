using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetWarehouses;

public sealed class GetWarehousesQueryHandler
    : IQueryHandler<GetWarehousesQuery, Result<PagedResult<WarehouseResponse>>>
{
    private readonly IWarehouseRepository _warehouses;

    public GetWarehousesQueryHandler(IWarehouseRepository warehouses) => _warehouses = warehouses;

    public async Task<Result<PagedResult<WarehouseResponse>>> Handle(
        GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var page = await _warehouses.SearchAsync(
            request.Search?.Trim(), request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(new PagedResult<WarehouseResponse>(
            page.Items.Select(w => w.ToResponse()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
