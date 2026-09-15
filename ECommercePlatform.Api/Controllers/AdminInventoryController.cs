using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.AdjustStock;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Application.Features.Inventory.GetInventoryList;
using ECommercePlatform.Application.Features.Inventory.GetInventoryMovements;
using ECommercePlatform.Application.Features.Inventory.GetLowStock;
using ECommercePlatform.Application.Features.Inventory.GetProductStock;
using ECommercePlatform.Application.Features.Inventory.ReceiveStock;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Admin operations for inventory and stock management.</summary>
[Authorize]
[Route("api/admin/inventory")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class AdminInventoryController : ApiControllerBase
{
    private readonly ILogger<AdminInventoryController> _logger;

    public AdminInventoryController(ISender sender, ILogger<AdminInventoryController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>1. GET /api/admin/inventory - List inventory with optional filters.</summary>
    [HttpGet]
    [HasPermission(Permissions.Inventory.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InventoryResponse>>> List(
        [FromQuery] GetInventoryListQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("List inventory action started.");

        try
        {
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List inventory action finished.");
        }
    }

    /// <summary>6. GET /api/admin/inventory/low-stock - List low stock inventory items.</summary>
    [HttpGet("low-stock")]
    [HasPermission(Permissions.Inventory.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InventoryResponse>>> LowStock(
        [FromQuery] GetLowStockQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Low stock inventory action started.");

        try
        {
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Low stock inventory action finished.");
        }
    }

    /// <summary>2. GET /api/admin/inventory/{productId} - Get product stock breakdown across warehouses.</summary>
    [HttpGet("{productId:guid}")]
    [HasPermission(Permissions.Inventory.View)]
    [ProducesResponseType(typeof(ProductStockDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductStockDetailResponse>> GetProductStock(
        Guid productId,
        [FromQuery] Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get product stock action started.");

        try
        {
            var query = new GetProductStockQuery(productId, warehouseId);
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Get product stock action finished.");
        }
    }

    /// <summary>3. POST /api/admin/inventory/{productId}/stock - Receive/add stock for a product.</summary>
    [HttpPost("{productId:guid}/stock")]
    [HasPermission(Permissions.Inventory.Add)]
    [ProducesResponseType(typeof(InventoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponse>> ReceiveStock(
        Guid productId,
        [FromBody] ReceiveStockRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive stock action started.");

        try
        {
            var command = new ReceiveStockCommand
            {
                ProductId = productId,
                WarehouseId = request.WarehouseId,
                Quantity = request.Quantity,
                Type = request.Type,
                Reason = request.Reason
            };

            var result = await Sender.Send(command, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Receive stock action finished.");
        }
    }

    /// <summary>4. POST /api/admin/inventory/{productId}/adjust - Adjust product stock level (positive/negative).</summary>
    [HttpPost("{productId:guid}/adjust")]
    [HasPermission(Permissions.Inventory.Adjust)]
    [ProducesResponseType(typeof(InventoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryResponse>> AdjustStock(
        Guid productId,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adjust stock action started.");

        try
        {
            var command = new AdjustStockCommand
            {
                ProductId = productId,
                WarehouseId = request.WarehouseId,
                Quantity = request.Quantity,
                Reason = request.Reason
            };

            var result = await Sender.Send(command, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Adjust stock action finished.");
        }
    }

    /// <summary>5. GET /api/admin/inventory/{productId}/movements - View stock movement history for a product.</summary>
    [HttpGet("{productId:guid}/movements")]
    [HasPermission(Permissions.Inventory.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryMovementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<InventoryMovementResponse>>> Movements(
        Guid productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get inventory movements action started.");

        try
        {
            var query = new GetInventoryMovementsQuery
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize
            };

            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Get inventory movements action finished.");
        }
    }
}
