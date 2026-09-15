using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Defines all data-access operations for roles and the user-to-role mapping.
/// </summary>
public interface IRoleRepository
{
    /// <summary>Retrieves every role defined in the system.</summary>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Retrieves the role names assigned to a specific user.</summary>
    Task<IReadOnlyCollection<string>> GetRolesForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Links a user to a role by inserting a row in the user-role mapping table.</summary>
    void AddAdminUserRole(Guid adminUserId, Guid roleId);

    /// <summary>Removes all role assignments for a specific user.</summary>
    void RemoveAdminUserRoles(Guid adminUserId);

    // ---- Role administration (roles CRUD). Additive members only; the
    // original three members above are untouched. ----

    /// <summary>Retrieves a single role by id, tracked for updates, or null.</summary>
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>True when another role already uses the name.</summary>
    Task<bool> NameExistsAsync(string roleName, Guid? excludingId, CancellationToken cancellationToken);

    /// <summary>Stages a new role row for insert.</summary>
    void Add(Role role);

    /// <summary>Stages a role row for delete.</summary>
    void Remove(Role role);

    /// <summary>Permission names granted to a role, ordered alphabetically.</summary>
    Task<IReadOnlyList<string>> GetPermissionNamesAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>Permission ids granted to a role.</summary>
    Task<IReadOnlyList<Guid>> GetPermissionIdsAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>Every defined permission, ordered by module then name.</summary>
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken);

    /// <summary>True when every id resolves to a defined permission.</summary>
    Task<bool> AllPermissionsExistAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken);

    /// <summary>Role-permission grant rows for a role.</summary>
    Task<IReadOnlyList<RolePermission>> GetRolePermissionsAsync(
        Guid roleId, CancellationToken cancellationToken);

    /// <summary>Stages one role-permission grant row for insert.</summary>
    void AddRolePermission(RolePermission rolePermission);

    /// <summary>Stages role-permission grant rows for delete.</summary>
    void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions);

    /// <summary>How many users currently hold the role.</summary>
    Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken);
}