using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.DeleteRole;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound(
                "admin.role_not_found", "The role could not be found."));
        }

        if (string.Equals(role.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            // Deleting Admin would orphan every AdminUserRole row and break all
            // Admin-gated endpoints plus the start-up permission seeder.
            return Result.Failure(Error.Conflict(
                "admin.role_protected", "The Admin role cannot be deleted."));
        }

        var userCount = await _roles.GetUserCountAsync(request.RoleId, cancellationToken);

        if (userCount > 0)
        {
            return Result.Failure(Error.Conflict(
                "admin.role_has_users",
                "This role is still assigned to users. Unassign them first."));
        }

        var grants = await _roles.GetRolePermissionsAsync(request.RoleId, cancellationToken);
        _roles.RemoveRolePermissions(grants);
        _roles.Remove(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
