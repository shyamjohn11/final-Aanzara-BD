using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Warehouses.CreateWarehouse;
using ECommercePlatform.Application.Features.Warehouses.DeleteWarehouse;
using ECommercePlatform.Application.Features.Warehouses.Dtos;
using ECommercePlatform.Application.Features.Warehouses.GetWarehouseById;
using ECommercePlatform.Application.Features.Warehouses.GetWarehouses;
using ECommercePlatform.Application.Features.Warehouses.UpdateWarehouse;
using ECommercePlatform.Application.Features.Warehouses.UpdateWarehouseStatus;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Admin operations for warehouses.</summary>
[Authorize]
[Route("api/admin/warehouses")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class AdminWarehousesController : ApiControllerBase
{
    private readonly ILogger<AdminWarehousesController> _logger;

    public AdminWarehousesController(ISender sender, ILogger<AdminWarehousesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Paged warehouse list with optional search and status filter.</summary>
    [HttpGet]
    [HasPermission(Permissions.Warehouse.View)]
    [ProducesResponseType(typeof(PagedResult<WarehouseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WarehouseResponse>>> List(
        [FromQuery] GetWarehousesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("List warehouses action started.");

        try
        {
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List warehouses action finished.");
        }
    }

    /// <summary>Gets a single warehouse by ID.</summary>
    [HttpGet("{warehouseId:guid}", Name = nameof(GetWarehouse))]
    [HasPermission(Permissions.Warehouse.View)]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseResponse>> GetWarehouse(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetWarehouse action started.");

        try
        {
            var result = await Sender.Send(new GetWarehouseByIdQuery(warehouseId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetWarehouse action finished.");
        }
    }

    /// <summary>Creates a new warehouse. Returns 201 with the created row.</summary>
    [HttpPost]
    [HasPermission(Permissions.Warehouse.Create)]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseResponse>> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create warehouse action started.");

        try
        {
            var command = new CreateWarehouseCommand
            {
                WarehouseName = request.WarehouseName,
                Address = request.Address,
                Status = request.Status
            };

            var result = await Sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return CreatedAtAction(
                nameof(GetWarehouse),
                new { warehouseId = result.Value.WarehouseId },
                result.Value);
        }
        finally
        {
            _logger.LogInformation("Create warehouse action finished.");
        }
    }

    /// <summary>Updates an existing warehouse by ID.</summary>
    [HttpPut("{warehouseId:guid}")]
    [HasPermission(Permissions.Warehouse.Update)]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseResponse>> Update(
        Guid warehouseId,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update warehouse action started.");

        try
        {
            var command = new UpdateWarehouseCommand
            {
                WarehouseId = warehouseId,
                WarehouseName = request.WarehouseName,
                Address = request.Address,
                Status = request.Status
            };

            var result = await Sender.Send(command, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Update warehouse action finished.");
        }
    }

    public sealed record UpdateWarehouseStatusRequest(string Status);

    /// <summary>Activates/deactivates a warehouse by ID.</summary>
    [HttpPatch("{warehouseId:guid}/status")]
    [HasPermission(Permissions.Warehouse.Update)]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseResponse>> UpdateStatus(
        Guid warehouseId,
        [FromBody] UpdateWarehouseStatusRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update warehouse status action started.");

        try
        {
            if (!Enum.TryParse<WarehouseStatus>(request.Status, ignoreCase: true, out var status))
            {
                return BadRequest($"Unknown status '{request.Status}'. Expected 'Active' or 'Inactive'.");
            }

            var result = await Sender.Send(
                new UpdateWarehouseStatusCommand(warehouseId, status), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Update warehouse status action finished.");
        }
    }

    /// <summary>Deletes a warehouse by ID. Refused if inventory items are assigned to it.</summary>
    [HttpDelete("{warehouseId:guid}")]
    [HasPermission(Permissions.Warehouse.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete warehouse action started.");

        try
        {
            var result = await Sender.Send(new DeleteWarehouseCommand(warehouseId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete warehouse action finished.");
        }
    }
}
