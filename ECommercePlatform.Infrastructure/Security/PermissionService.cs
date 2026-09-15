using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Security;

public sealed class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _db;

    public PermissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        return await _db.AdminUserRoles
            .Where(aur => aur.AdminUserId == userId)
            .SelectMany(aur => _db.RolePermissions.Where(rp => rp.RoleId == aur.RoleId))
            .AnyAsync(rp => rp.Permission.PermissionName == permission, cancellationToken);
    }

    public async Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await _db.AdminUserRoles
            .Where(aur => aur.AdminUserId == userId)
            .SelectMany(aur => _db.RolePermissions.Where(rp => rp.RoleId == aur.RoleId))
            .Select(rp => rp.Permission.PermissionName)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
