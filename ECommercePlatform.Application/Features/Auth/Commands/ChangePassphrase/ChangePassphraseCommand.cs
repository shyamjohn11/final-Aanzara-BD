using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.ChangePassphrase;

public sealed record ChangePassphraseCommand : AuthenticatedCommand, ICommand<Result>
{
    [Required]
    [MaxLength(256)]
    public string CurrentPassphrase { get; init; } = string.Empty;

    [Required]
    [MinLength(12, ErrorMessage = "The {0} field must be at least {1} characters.")]
    [MaxLength(256)]
    public string NewPassphrase { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassphrase), ErrorMessage = "The passphrase and confirmation do not match.")]
    public string ConfirmNewPassphrase { get; init; } = string.Empty;
}
