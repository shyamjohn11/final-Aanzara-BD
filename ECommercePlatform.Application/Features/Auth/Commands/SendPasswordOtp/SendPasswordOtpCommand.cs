using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.SendPasswordOtp;

/// <summary>
/// Triggers generation of a secure 6-digit OTP sent to the user's email for password change/reset.
/// </summary>
public sealed record SendPasswordOtpCommand : ICommand<Result>
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;
}
