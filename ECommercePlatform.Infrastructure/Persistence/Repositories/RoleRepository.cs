using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete implementation of IRoleRepository using Entity Framework Core.
/// Handles fetching roles and managing the user-to-role mapping table.
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _database;

    public RoleRepository(ApplicationDbContext database) => _database = database;

    /// <summary>Retrieves all roles ordered alphabetically by name.</summary>
    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken)
        => await _database.Roles
            .AsNoTracking()
            .OrderBy(role => role.RoleName)
            .ToListAsync(cancellationToken);

    /// <summary>Returns the names of all roles assigned to the specified user.</summary>
    public async Task<IReadOnlyCollection<string>> GetRolesForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _database.AdminUserRoles
            .AsNoTracking()
            .Where(mapping => mapping.AdminUserId == userId)
            .Select(mapping => mapping.Role.RoleName)
            .Distinct()
            .OrderBy(roleName => roleName)
            .ToListAsync(cancellationToken);

    /// <summary>Creates a mapping between a user and a role in the database.</summary>
    public void AddAdminUserRole(Guid adminUserId, Guid roleId)
        => _database.AdminUserRoles.Add(new AdminUserRole
        {
            AdminUserId = adminUserId,
            RoleId = roleId
        });

    /// <summary>Removes all role assignments for a specific user.</summary>
    public void RemoveAdminUserRoles(Guid adminUserId)
    {
        var existing = _database.AdminUserRoles
            .Where(mapping => mapping.AdminUserId == adminUserId)
            .ToList();
        _database.AdminUserRoles.RemoveRange(existing);
    }

    // ---- Role administration (roles CRUD). ----

    /// <inheritdoc />
    public async Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken)
        => await _database.Roles
            .FirstOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> NameExistsAsync(
        string roleName, Guid? excludingId, CancellationToken cancellationToken)
        => await _database.Roles.AnyAsync(
            role => role.RoleName == roleName
                && (!excludingId.HasValue || role.RoleId != excludingId.Value),
            cancellationToken);

    /// <inheritdoc />
    public void Add(Role role) => _database.Roles.Add(role);

    /// <inheritdoc />
    public void Remove(Role role) => _database.Roles.Remove(role);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetPermissionNamesAsync(
        Guid roleId, CancellationToken cancellationToken)
        => await _database.RolePermissions
            .AsNoTracking()
            .Where(grant => grant.RoleId == roleId)
            .Select(grant => grant.Permission.PermissionName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetPermissionIdsAsync(
        Guid roleId, CancellationToken cancellationToken)
        => await _database.RolePermissions
            .AsNoTracking()
            .Where(grant => grant.RoleId == roleId)
            .Select(grant => grant.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(
        CancellationToken cancellationToken)
        => await _database.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Module)
            .ThenBy(permission => permission.PermissionName)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken)
        => await _database.Roles
            .FirstOrDefaultAsync(role => role.RoleName == roleName, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> AllPermissionsExistAsync(
        IEnumerable<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var ids = permissionIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return true;
        }

        var matching = await _database.Permissions
            .AsNoTracking()
            .Where(permission => ids.Contains(permission.PermissionId))
            .Select(permission => permission.PermissionId)
            .ToListAsync(cancellationToken);

        return matching.Count == ids.Count;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RolePermission>> GetRolePermissionsAsync(
        Guid roleId, CancellationToken cancellationToken)
        => await _database.RolePermissions
            .Where(grant => grant.RoleId == roleId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void AddRolePermission(RolePermission rolePermission)
        => _database.RolePermissions.Add(rolePermission);

    /// <inheritdoc />
    public void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions)
        => _database.RolePermissions.RemoveRange(rolePermissions);

    /// <inheritdoc />
    public async Task<int> GetUserCountAsync(Guid roleId, CancellationToken cancellationToken)
        => await _database.AdminUserRoles
            .AsNoTracking()
            .CountAsync(mapping => mapping.RoleId == roleId, cancellationToken);
}