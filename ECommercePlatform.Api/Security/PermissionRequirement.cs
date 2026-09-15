using Microsoft.AspNetCore.Authorization;

namespace ECommercePlatform.Api.Security;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
