// Infrastructure/Security/RoleService.cs
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Infrastructure.Security;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roles;

    public RoleService(IRoleRepository roles) => _roles = roles;

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken)
        => _roles.GetAllAsync(cancellationToken);

    public Task<IReadOnlyCollection<string>> GetRolesForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => _roles.GetRolesForUserAsync(userId, cancellationToken);
}
