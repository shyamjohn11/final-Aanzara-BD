using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.UpdateRole;

public sealed record UpdateRoleCommand : ICommand<Result<RoleAdminResponse>>
{
    public Guid RoleId { get; init; }

    [Required]
    [MaxLength(100)]
    public string RoleName { get; init; } = string.Empty;

    public IReadOnlyList<Guid>? PermissionIds { get; init; }
}
