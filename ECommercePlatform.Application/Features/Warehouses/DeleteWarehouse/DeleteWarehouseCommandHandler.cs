using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Warehouses.DeleteWarehouse;

public sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand, Result>
{
    private readonly IWarehouseRepository _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteWarehouseCommandHandler> _logger;

    public DeleteWarehouseCommandHandler(
        IWarehouseRepository warehouses,
        IUnitOfWork unitOfWork,
        ILogger<DeleteWarehouseCommandHandler> logger)
    {
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouses.GetByIdAsync(request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure(WarehouseErrors.WarehouseNotFound);
        }

        if (await _warehouses.HasInventoryAsync(request.WarehouseId, cancellationToken))
        {
            _logger.LogInformation(
                "Refused to delete warehouse {WarehouseId}: it still has inventory records assigned.", request.WarehouseId);

            return Result.Failure(WarehouseErrors.WarehouseHasInventory);
        }

        _warehouses.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseId} deleted.", request.WarehouseId);

        return Result.Success();
    }
}
