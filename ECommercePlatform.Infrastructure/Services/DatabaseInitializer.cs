using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Infrastructure.Services;

/// <summary>
/// Applies pending migrations and seeds reference data at start-up. Run
/// explicitly rather than from a static constructor so failures surface as
/// start-up errors with a readable log.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly SeedOptions _seedOptions;

    public DatabaseInitializer(
        ApplicationDbContext db,
        ILogger<DatabaseInitializer> logger,
        IOptions<SeedOptions> seedOptions)
    {
        _db = db;
        _logger = logger;
        _seedOptions = seedOptions.Value;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        // SQL Server may still be starting (e.g. right after boot): retry
        // transient connection failures instead of crashing the API with
        // exit code 1 on the first attempt. Only SqlException is retried —
        // anything else (bad migration, model mismatch, ...) still fails
        // fast so real problems stay loud.
        const int maxAttempts = 6;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await ApplyPendingMigrationsAsync(cancellationToken);
                break;
            }
            catch (SqlException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Database unreachable (attempt {Attempt}/{MaxAttempts}). Retrying in 5 seconds...",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await SeedPermissionsAsync(cancellationToken);
    }

    private async Task ApplyPendingMigrationsAsync(CancellationToken cancellationToken)
    {
        var pending = (await _db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

        if (pending.Count == 0)
        {
            _logger.LogInformation("Database schema is up to date.");
        }
        else
        {
            _logger.LogInformation(
                "Applying {Count} pending migration(s): {Migrations}", pending.Count, pending);

            await _db.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Migrations applied.");
        }
    }

    /// <summary>
    /// Ensures every <see cref="Permissions"/> constant exists as a row, that the
    /// "Admin" role holds all of them, and — if configured — that the given user
    /// has that role. Every step is idempotent: safe to run on every start-up,
    /// against a database that already has some or all of this data.
    /// </summary>
    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var definedPermissions = new (string Name, string Module)[]
        {
            (Permissions.Category.View, nameof(Permissions.Category)),
            (Permissions.Category.Create, nameof(Permissions.Category)),
            (Permissions.Category.Update, nameof(Permissions.Category)),
            (Permissions.Category.Delete, nameof(Permissions.Category)),

            (Permissions.Product.View, nameof(Permissions.Product)),
            (Permissions.Product.Create, nameof(Permissions.Product)),
            (Permissions.Product.Update, nameof(Permissions.Product)),
            (Permissions.Product.Delete, nameof(Permissions.Product)),

            (Permissions.Brand.View, nameof(Permissions.Brand)),
            (Permissions.Brand.Create, nameof(Permissions.Brand)),
            (Permissions.Brand.Update, nameof(Permissions.Brand)),
            (Permissions.Brand.Delete, nameof(Permissions.Brand)),

            (Permissions.Warehouse.View, nameof(Permissions.Warehouse)),
            (Permissions.Warehouse.Create, nameof(Permissions.Warehouse)),
            (Permissions.Warehouse.Update, nameof(Permissions.Warehouse)),
            (Permissions.Warehouse.Delete, nameof(Permissions.Warehouse)),

            (Permissions.Inventory.View, nameof(Permissions.Inventory)),
            (Permissions.Inventory.Add, nameof(Permissions.Inventory)),
            (Permissions.Inventory.Adjust, nameof(Permissions.Inventory)),

            (Permissions.Wholesale.View, nameof(Permissions.Wholesale)),
            (Permissions.Wholesale.Update, nameof(Permissions.Wholesale)),
        };

        // --- Permissions: insert any name that isn't already a row. ---
        var existingPermissionNames = await _db.Permissions
            .Select(p => p.PermissionName)
            .ToListAsync(cancellationToken);

        var missingPermissions = definedPermissions
            .Where(p => !existingPermissionNames.Contains(p.Name))
            .Select(p => new Permission
            {
                PermissionId = Guid.NewGuid(),
                PermissionName = p.Name,
                Module = p.Module
            })
            .ToList();

        if (missingPermissions.Count > 0)
        {
            _db.Permissions.AddRange(missingPermissions);
            _logger.LogInformation(
                "Seeding {Count} missing permission(s).", missingPermissions.Count);
        }

        // --- Roles: make sure "Admin" exists. ---
        var adminRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.RoleName == Roles.Admin, cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role { RoleId = Guid.NewGuid(), RoleName = Roles.Admin };
            _db.Roles.Add(adminRole);
            _logger.LogInformation("Seeding '{Role}' role.", Roles.Admin);
        }

        // Persist so the two lookups below can see rows added above.
        await _db.SaveChangesAsync(cancellationToken);

        // --- RolePermissions: grant every permission to Admin. ---
        var allPermissions = await _db.Permissions.ToListAsync(cancellationToken);

        var grantedPermissionIds = await _db.RolePermissions
            .Where(rp => rp.RoleId == adminRole.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var missingGrants = allPermissions
            .Where(p => !grantedPermissionIds.Contains(p.PermissionId))
            .Select(p => new RolePermission { RoleId = adminRole.RoleId, PermissionId = p.PermissionId })
            .ToList();

        if (missingGrants.Count > 0)
        {
            _db.RolePermissions.AddRange(missingGrants);
            _logger.LogInformation(
                "Granting {Count} permission(s) to '{Role}'.", missingGrants.Count, Roles.Admin);
        }

        // --- Customer role: read-only catalog access for the storefront. ---
        // New registrants land in this role (UserService), so without these grants
        // every customer-facing catalog call would come back 403.
        var customerRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.RoleName == Roles.Customer, cancellationToken);

        if (customerRole is null)
        {
            customerRole = new Role { RoleId = Guid.NewGuid(), RoleName = Roles.Customer };
            _db.Roles.Add(customerRole);
            _logger.LogInformation("Seeding '{Role}' role.", Roles.Customer);
        }

        // Persist so the lookup below can see the role added above.
        await _db.SaveChangesAsync(cancellationToken);

        var customerPermissionNames = new[]
        {
            Permissions.Category.View,
            Permissions.Product.View,
            Permissions.Brand.View
        };

        var customerPermissions = await _db.Permissions
            .Where(p => customerPermissionNames.Contains(p.PermissionName))
            .ToListAsync(cancellationToken);

        var customerGrantedIds = await _db.RolePermissions
            .Where(rp => rp.RoleId == customerRole.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var customerMissingGrants = customerPermissions
            .Where(p => !customerGrantedIds.Contains(p.PermissionId))
            .Select(p => new RolePermission { RoleId = customerRole.RoleId, PermissionId = p.PermissionId })
            .ToList();

        if (customerMissingGrants.Count > 0)
        {
            _db.RolePermissions.AddRange(customerMissingGrants);
            _logger.LogInformation(
                "Granting {Count} permission(s) to '{Role}'.", customerMissingGrants.Count, Roles.Customer);
        }

        // --- AdminUserRole: put the configured user (if any) into Admin. ---
        if (!string.IsNullOrWhiteSpace(_seedOptions.AdminEmail))
        {
            var adminUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == _seedOptions.AdminEmail, cancellationToken);

            if (adminUser is null)
            {
                _logger.LogWarning(
                    "Seed:AdminEmail is set to '{Email}', but no user with that email exists yet. "
                    + "Register that user, then restart so this can grant the Admin role.",
                    _seedOptions.AdminEmail);
            }
            else
            {
                var alreadyAdmin = await _db.AdminUserRoles.AnyAsync(
                    aur => aur.AdminUserId == adminUser.UserId && aur.RoleId == adminRole.RoleId,
                    cancellationToken);

                if (!alreadyAdmin)
                {
                    _db.AdminUserRoles.Add(new AdminUserRole
                    {
                        AdminUserId = adminUser.UserId,
                        RoleId = adminRole.RoleId
                    });

                    _logger.LogInformation(
                        "Granting '{Role}' role to '{Email}'.", Roles.Admin, _seedOptions.AdminEmail);
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Bound from the "Seed" section. Optional by design: leave AdminEmail unset in
/// any environment where roles should be assigned by hand instead of by config.
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>
    /// Email of an already-registered user to grant the Admin role to on every
    /// start-up. Safe to leave empty; nothing here runs if it is.
    /// </summary>
    public string? AdminEmail { get; set; }
}