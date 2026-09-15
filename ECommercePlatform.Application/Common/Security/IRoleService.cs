using ECommercePlatform.Domain.Entities;

// Application/Common/Security/IRoleService.cs
namespace ECommercePlatform.Application.Common.Security;

public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetRolesForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
