using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.UpdateWarehouseStatus;

public sealed record UpdateWarehouseStatusCommand(
    Guid WarehouseId,
    WarehouseStatus Status) : ICommand<Result<WarehouseResponse>>;
