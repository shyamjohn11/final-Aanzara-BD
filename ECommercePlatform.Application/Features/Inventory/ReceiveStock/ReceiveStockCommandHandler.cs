using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Application.Features.Warehouses;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Inventory.ReceiveStock;

public sealed class ReceiveStockCommandHandler
    : ICommandHandler<ReceiveStockCommand, Result<InventoryResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ReceiveStockCommandHandler> _logger;

    public ReceiveStockCommandHandler(
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<ReceiveStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<InventoryResponse>> Handle(
        ReceiveStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Result.Failure<InventoryResponse>(InventoryErrors.InvalidQuantity);
        }

        if (await _productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            return Result.Failure<InventoryResponse>(CatalogErrors.ProductNotFound);
        }

        if (!await _warehouseRepository.ExistsAsync(request.WarehouseId, cancellationToken))
        {
            return Result.Failure<InventoryResponse>(WarehouseErrors.WarehouseNotFound);
        }

        var inventory = await _inventoryRepository.GetByProductAndWarehouseAsync(
            request.ProductId, request.WarehouseId, cancellationToken);

        if (inventory is null)
        {
            inventory = new Domain.Entities.Inventory
            {
                InventoryId = Guid.NewGuid(),
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                StockQuantity = request.Quantity,
                ReservedQuantity = 0,
                ReorderLevel = 0,
                DispatchEstimateDays = 0
            };

            _inventoryRepository.Add(inventory);
        }
        else
        {
            inventory.StockQuantity += request.Quantity;
        }

        var movement = new InventoryMovement
        {
            MovementId = Guid.NewGuid(),
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Type = request.Type,
            Quantity = request.Quantity,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Stock received" : request.Reason.Trim(),
            CreatedByUserId = _currentUser.UserId
        };

        _inventoryRepository.AddMovement(movement);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Received {Quantity} stock for Product {ProductId} at Warehouse {WarehouseId}.",
            request.Quantity, request.ProductId, request.WarehouseId);

        var resultInventory = await _inventoryRepository.GetByProductAndWarehouseAsync(
            request.ProductId, request.WarehouseId, cancellationToken);

        return Result.Success((resultInventory ?? inventory).ToResponse());
    }
}
