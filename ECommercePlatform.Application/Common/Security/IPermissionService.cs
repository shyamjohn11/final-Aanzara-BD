namespace ECommercePlatform.Application.Common.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
