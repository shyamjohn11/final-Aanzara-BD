using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetWarehouseById;

public sealed record GetWarehouseByIdQuery(Guid WarehouseId) : IQuery<Result<WarehouseResponse>>;
