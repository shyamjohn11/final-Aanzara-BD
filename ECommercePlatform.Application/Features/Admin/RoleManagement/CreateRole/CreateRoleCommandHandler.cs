using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.CreateRole;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, Result<RoleAdminResponse>>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleAdminResponse>> Handle(
        CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var name = request.RoleName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<RoleAdminResponse>(Error.Validation(
                "admin.role_name_required", "Role name is required."));
        }

        if (await _roles.NameExistsAsync(name, excludingId: null, cancellationToken))
        {
            return Result.Failure<RoleAdminResponse>(Error.Conflict(
                "admin.role_name_taken", $"Role name '{name}' is already in use."));
        }

        var permissionIds = (request.PermissionIds ?? []).Distinct().ToList();

        if (!await _roles.AllPermissionsExistAsync(permissionIds, cancellationToken))
        {
            return Result.Failure<RoleAdminResponse>(Error.Validation(
                "admin.permission_unknown", "One or more permission ids are unknown."));
        }

        var role = new Role { RoleId = Guid.NewGuid(), RoleName = name };
        _roles.Add(role);

        foreach (var permissionId in permissionIds)
        {
            _roles.AddRolePermission(new RolePermission { RoleId = role.RoleId, PermissionId = permissionId });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RoleAdminResponse
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Permissions = await _roles.GetPermissionNamesAsync(role.RoleId, cancellationToken),
            UserCount = 0,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        });
    }
}
