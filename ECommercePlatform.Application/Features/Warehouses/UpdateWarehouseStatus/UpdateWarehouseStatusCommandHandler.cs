using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Warehouses.UpdateWarehouseStatus;

public sealed class UpdateWarehouseStatusCommandHandler
    : ICommandHandler<UpdateWarehouseStatusCommand, Result<WarehouseResponse>>
{
    private readonly IWarehouseRepository _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWarehouseStatusCommandHandler> _logger;

    public UpdateWarehouseStatusCommandHandler(
        IWarehouseRepository warehouses,
        IUnitOfWork unitOfWork,
        ILogger<UpdateWarehouseStatusCommandHandler> logger)
    {
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WarehouseResponse>> Handle(
        UpdateWarehouseStatusCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouses.GetByIdAsync(request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure<WarehouseResponse>(WarehouseErrors.WarehouseNotFound);
        }

        warehouse.Status = request.Status;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Warehouse {WarehouseId} status changed to {Status}.",
            warehouse.WarehouseId,
            warehouse.Status);

        return Result.Success(warehouse.ToResponse());
    }
}
