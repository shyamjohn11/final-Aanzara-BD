using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand : ClientAwareCommand, ICommand<Result<AuthResponse>>
{
    // Optional: the API controller also accepts the HttpOnly refresh cookie.
    [MaxLength(200)]
    public string RefreshToken { get; init; } = string.Empty;
}
