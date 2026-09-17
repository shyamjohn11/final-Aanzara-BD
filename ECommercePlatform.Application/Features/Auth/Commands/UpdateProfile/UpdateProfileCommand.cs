using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateProfileCommand : AuthenticatedCommand, ICommand<Result<UserProfileResponse>>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; init; }

    public DateTimeOffset? DateOfBirth { get; init; }

    [MaxLength(30)]
    public string? Gender { get; init; }
}
