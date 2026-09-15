using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Admin.RoleManagement;
using ECommercePlatform.Application.Features.Admin.RoleManagement.CreateRole;
using ECommercePlatform.Application.Features.Admin.RoleManagement.DeleteRole;
using ECommercePlatform.Application.Features.Admin.RoleManagement.GetPermissionsCatalog;
using ECommercePlatform.Application.Features.Admin.RoleManagement.GetRoleById;
using ECommercePlatform.Application.Features.Admin.RoleManagement.UpdateRole;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Administration endpoints for roles. Listing stays exactly as it
/// was; detail/create/update/delete below reuse the same tables.</summary>
[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/roles")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _roles;
    private readonly ISender _sender;

    public RolesController(IRoleService roles, ISender sender)
    {
        _roles = roles;
        _sender = sender;
    }

    /// <summary>Returns all defined roles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleListItemResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var roles = await _roles.GetAllAsync(cancellationToken);
        var response = roles
            .Select(role => new RoleListItemResponse(role.RoleId, role.RoleName))
            .ToList();

        return Ok(response);
    }

    /// <summary>One role with its permissions and user count.</summary>
    [HttpGet("{roleId:guid}", Name = "GetAdminRole")]
    [ProducesResponseType(typeof(RoleAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleAdminResponse>> GetById(
        Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoleByIdQuery(roleId), cancellationToken);

        return result.IsFailure
            ? ProblemFrom(result.Error!)
            : Ok(result.Value);
    }

    /// <summary>Creates a role with an optional permission set. Returns 201.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoleAdminResponse>> Create(
        [FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return result.IsFailure
            ? ProblemFrom(result.Error!)
            : CreatedAtAction(nameof(GetById), new { roleId = result.Value.RoleId }, result.Value);
    }

    /// <summary>Renames a role and/or replaces its permission set.</summary>
    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(typeof(RoleAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoleAdminResponse>> Update(
        Guid roleId, [FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { RoleId = roleId }, cancellationToken);

        return result.IsFailure
            ? ProblemFrom(result.Error!)
            : Ok(result.Value);
    }

    /// <summary>
    /// Deletes a role. Refused for Admin and while users still hold the role.
    /// There is no status toggle: the Role table has no status column, so
    /// presence of the row is the active state.
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteRoleCommand(roleId), cancellationToken);

        return result.IsFailure
            ? ProblemFrom(result.Error!)
            : NoContent();
    }

    /// <summary>Every defined permission, for the role create/edit pickers.</summary>
    [HttpGet("/api/admin/permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionCatalogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionCatalogResponse>>> Permissions(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPermissionsCatalogQuery(), cancellationToken);

        return result.IsFailure
            ? ProblemFrom(result.Error!)
            : Ok(result.Value);
    }

    private ActionResult ProblemFrom(ECommercePlatform.Domain.Errors.Error error)
    {
        var statusCode = error.Type.ToStatusCode();

        return StatusCode(statusCode, new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToTitle(),
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{Request.Method} {Request.Path}"
        });
    }
}

/// <summary>Role fields returned by the administration read endpoint.</summary>
public sealed record RoleListItemResponse(Guid RoleId, string RoleName);
