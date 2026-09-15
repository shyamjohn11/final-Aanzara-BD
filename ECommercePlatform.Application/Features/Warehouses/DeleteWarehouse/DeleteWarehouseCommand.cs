using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.DeleteWarehouse;

public sealed record DeleteWarehouseCommand(Guid WarehouseId) : ICommand<Result>;
