using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Application.Features.Warehouses;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockCommandHandler
    : ICommandHandler<AdjustStockCommand, Result<InventoryResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AdjustStockCommandHandler> _logger;

    public AdjustStockCommandHandler(
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<AdjustStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<InventoryResponse>> Handle(
        AdjustStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity == 0)
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
            if (request.Quantity < 0)
            {
                return Result.Failure<InventoryResponse>(InventoryErrors.StockCannotBeNegative);
            }

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
            if (inventory.StockQuantity + request.Quantity < 0)
            {
                return Result.Failure<InventoryResponse>(InventoryErrors.StockCannotBeNegative);
            }

            inventory.StockQuantity += request.Quantity;
        }

        var movement = new InventoryMovement
        {
            MovementId = Guid.NewGuid(),
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Type = InventoryMovementType.ADJUSTMENT,
            Quantity = request.Quantity,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Stock adjustment" : request.Reason.Trim(),
            CreatedByUserId = _currentUser.UserId
        };

        _inventoryRepository.AddMovement(movement);

        var availableAfter =
            inventory.StockQuantity - inventory.ReservedQuantity;
        if (inventory.ReorderLevel > 0 && availableAfter <= inventory.ReorderLevel)
        {
            var product = await _productRepository.GetByIdAsync(
                request.ProductId, cancellationToken);
            NotificationEmitter.Emit(
                _notifications,
                "inventory",
                $"Low stock: {product?.ProductName ?? request.ProductId.ToString()}",
                $"Available {availableAfter} at reorder level {inventory.ReorderLevel}.",
                "/admin/inventory");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Adjusted stock by {Quantity} for Product {ProductId} at Warehouse {WarehouseId}.",
            request.Quantity, request.ProductId, request.WarehouseId);

        var resultInventory = await _inventoryRepository.GetByProductAndWarehouseAsync(
            request.ProductId, request.WarehouseId, cancellationToken);

        return Result.Success((resultInventory ?? inventory).ToResponse());
    }
}
