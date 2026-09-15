using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.ChangePasswordWithOtp;

/// <summary>
/// Changes the user's password using the OTP received via email.
/// </summary>
public sealed record ChangePasswordWithOtpCommand : ICommand<Result>
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "The OTP must be exactly 6 digits.")]
    public string Otp { get; init; } = string.Empty;

    [Required]
    [MinLength(12, ErrorMessage = "The {0} field must be at least {1} characters.")]
    [MaxLength(256)]
    public string NewPassphrase { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassphrase), ErrorMessage = "The passphrase and confirmation do not match.")]
    public string ConfirmNewPassphrase { get; init; } = string.Empty;
}
