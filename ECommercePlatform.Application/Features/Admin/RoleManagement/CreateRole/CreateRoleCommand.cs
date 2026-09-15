using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.CreateRole;

public sealed record CreateRoleCommand : ICommand<Result<RoleAdminResponse>>
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; init; } = string.Empty;

    public IReadOnlyList<Guid>? PermissionIds { get; init; }
}
