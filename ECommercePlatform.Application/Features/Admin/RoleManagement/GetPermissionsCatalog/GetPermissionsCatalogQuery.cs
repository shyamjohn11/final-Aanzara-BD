using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.GetPermissionsCatalog;

public sealed record GetPermissionsCatalogQuery
    : IQuery<Result<IReadOnlyList<PermissionCatalogResponse>>>;

public sealed class GetPermissionsCatalogQueryHandler
    : IQueryHandler<GetPermissionsCatalogQuery, Result<IReadOnlyList<PermissionCatalogResponse>>>
{
    private readonly IRoleRepository _roles;

    public GetPermissionsCatalogQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<Result<IReadOnlyList<PermissionCatalogResponse>>> Handle(
        GetPermissionsCatalogQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _roles.GetAllPermissionsAsync(cancellationToken);

        return Result.Success<IReadOnlyList<PermissionCatalogResponse>>(
            permissions.Select(p => new PermissionCatalogResponse
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                Module = p.Module
            }).ToArray());
    }
}
