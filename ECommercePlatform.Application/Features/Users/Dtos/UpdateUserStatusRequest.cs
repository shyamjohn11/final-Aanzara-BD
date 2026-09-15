using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Application.Features.Users.Dtos;

public sealed class UpdateUserStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}