using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetWarehouseById;

public sealed class GetWarehouseByIdQueryHandler
    : IQueryHandler<GetWarehouseByIdQuery, Result<WarehouseResponse>>
{
    private readonly IWarehouseRepository _warehouses;

    public GetWarehouseByIdQueryHandler(IWarehouseRepository warehouses) => _warehouses = warehouses;

    public async Task<Result<WarehouseResponse>> Handle(
        GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouses.GetByIdAsync(request.WarehouseId, cancellationToken);

        return warehouse is null
            ? Result.Failure<WarehouseResponse>(WarehouseErrors.WarehouseNotFound)
            : Result.Success(warehouse.ToResponse());
    }
}
