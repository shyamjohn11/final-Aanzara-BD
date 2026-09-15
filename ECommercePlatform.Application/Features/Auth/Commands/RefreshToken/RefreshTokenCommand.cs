using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand : ClientAwareCommand, ICommand<Result<AuthResponse>>
{
    [Required]
    [MaxLength(200)]
    public string RefreshToken { get; init; } = string.Empty;
}
