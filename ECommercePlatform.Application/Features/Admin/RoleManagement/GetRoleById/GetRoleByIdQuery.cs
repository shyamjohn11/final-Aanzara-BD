using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.GetRoleById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<Result<RoleAdminResponse>>;

public sealed class GetRoleByIdQueryHandler
    : IQueryHandler<GetRoleByIdQuery, Result<RoleAdminResponse>>
{
    private readonly IRoleRepository _roles;

    public GetRoleByIdQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<RoleAdminResponse>> Handle(
        GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleAdminResponse>(Error.NotFound(
                "admin.role_not_found", "The role could not be found."));
        }

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
