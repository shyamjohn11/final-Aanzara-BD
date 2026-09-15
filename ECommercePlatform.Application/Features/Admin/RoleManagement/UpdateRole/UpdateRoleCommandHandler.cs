using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.UpdateRole;

public sealed class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand, Result<RoleAdminResponse>>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleAdminResponse>> Handle(
        UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Tracked fetch: mutations below are picked up by SaveChanges.
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleAdminResponse>(Error.NotFound(
                "admin.role_not_found", "The role could not be found."));
        }

        var name = request.RoleName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<RoleAdminResponse>(Error.Validation(
                "admin.role_name_required", "Role name is required."));
        }

        var isAdminRole = string.Equals(role.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase);

        if (isAdminRole && !string.Equals(name, role.RoleName, StringComparison.Ordinal))
        {
            // [Authorize(Roles = "Admin")] and the start-up seeder both key off
            // this exact name — renaming it would lock every admin out.
            return Result.Failure<RoleAdminResponse>(Error.Conflict(
                "admin.role_protected", "The Admin role cannot be renamed."));
        }

        if (await _roles.NameExistsAsync(name, request.RoleId, cancellationToken))
        {
            return Result.Failure<RoleAdminResponse>(Error.Conflict(
                "admin.role_name_taken", $"Role name '{name}' is already in use."));
        }

        var permissionIds = (request.PermissionIds
            ?? await _roles.GetPermissionIdsAsync(request.RoleId, cancellationToken))
            .Distinct()
            .ToList();

        if (isAdminRole && permissionIds.Count == 0)
        {
            return Result.Failure<RoleAdminResponse>(Error.Conflict(
                "admin.role_protected", "The Admin role must keep its permissions."));
        }

        if (!await _roles.AllPermissionsExistAsync(permissionIds, cancellationToken))
        {
            return Result.Failure<RoleAdminResponse>(Error.Validation(
                "admin.permission_unknown", "One or more permission ids are unknown."));
        }

        role.RoleName = name;

        // Grants are replaced wholesale: remove current, add requested.
        var current = await _roles.GetRolePermissionsAsync(request.RoleId, cancellationToken);
        _roles.RemoveRolePermissions(current);

        foreach (var permissionId in permissionIds)
        {
            _roles.AddRolePermission(new RolePermission { RoleId = request.RoleId, PermissionId = permissionId });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RoleAdminResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Permissions = await _roles.GetPermissionNamesAsync(role.RoleId, cancellationToken),
            UserCount = await _roles.GetUserCountAsync(role.RoleId, cancellationToken),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        });
    }
}
