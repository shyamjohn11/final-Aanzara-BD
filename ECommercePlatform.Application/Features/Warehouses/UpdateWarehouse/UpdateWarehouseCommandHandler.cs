using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Warehouses.UpdateWarehouse;

public sealed class UpdateWarehouseCommandHandler
    : ICommandHandler<UpdateWarehouseCommand, Result<WarehouseResponse>>
{
    private readonly IWarehouseRepository _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWarehouseCommandHandler> _logger;

    public UpdateWarehouseCommandHandler(
        IWarehouseRepository warehouses,
        IUnitOfWork unitOfWork,
        ILogger<UpdateWarehouseCommandHandler> logger)
    {
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WarehouseResponse>> Handle(
        UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouses.GetByIdAsync(request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.WarehouseNotFound);
        }

        var name = request.WarehouseName.Trim();

        if (await _warehouses.NameExistsAsync(name, excludingId: request.WarehouseId, cancellationToken))
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.WarehouseNameTaken(name));
        }

        warehouse.WarehouseName = name;
        warehouse.Address = request.Address?.Trim();
        warehouse.City = request.City?.Trim();
        warehouse.State = request.State?.Trim();
        warehouse.Pincode = request.Pincode?.Trim();
        warehouse.Latitude = request.Latitude;
        warehouse.Longitude = request.Longitude;
        warehouse.Status = request.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseId} updated.", warehouse.WarehouseId);

        return Result.Success(warehouse.ToResponse());
    }
}
