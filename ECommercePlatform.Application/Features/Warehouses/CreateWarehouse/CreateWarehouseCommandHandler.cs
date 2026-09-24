using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Warehouses.CreateWarehouse;

public sealed class CreateWarehouseCommandHandler
    : ICommandHandler<CreateWarehouseCommand, Result<WarehouseResponse>>
{
    private readonly IWarehouseRepository _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateWarehouseCommandHandler> _logger;

    public CreateWarehouseCommandHandler(
        IWarehouseRepository warehouses,
        IUnitOfWork unitOfWork,
        ILogger<CreateWarehouseCommandHandler> logger)
    {
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WarehouseResponse>> Handle(
        CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var name = request.WarehouseName.Trim();

        if (await _warehouses.NameExistsAsync(name, excludingId: null, cancellationToken))
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.WarehouseNameTaken(name));
        }

        var warehouse = new Warehouse
        {
            WarehouseId = Guid.NewGuid(),
            WarehouseName = name,
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            Pincode = request.Pincode?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = request.Status
        };

        _warehouses.Add(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseId} ({WarehouseName}) created.", warehouse.WarehouseId, name);

        return Result.Success(warehouse.ToResponse());
    }
}
