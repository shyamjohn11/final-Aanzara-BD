namespace ECommercePlatform.Application.Features.Admin.RoleManagement;

public sealed record RoleAdminResponse
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; init; } = [];
    public int UserCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record PermissionCatalogResponse
{
    public Guid PermissionId { get; init; }
    public string PermissionName { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
}
