using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand : ClientAwareCommand, ICommand<Result<AuthResponse>>
{
    [Required]
    [MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string? Phone { get; init; }

    [Required]
    [MinLength(12, ErrorMessage = "The {0} field must be at least {1} characters.")]
    [MaxLength(256)]
    public string Passphrase { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Passphrase), ErrorMessage = "The passphrase and confirmation do not match.")]
    public string ConfirmPassphrase { get; init; } = string.Empty;
}