using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Application.Features.Users.Dtos;

public sealed class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}