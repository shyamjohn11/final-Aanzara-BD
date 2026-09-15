using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.Login;

/// <summary>
/// A command rather than a query despite "reading" credentials: it writes a
/// session and may rewrite the stored hash when the work factor is outdated.
/// </summary>
public sealed record LoginCommand : ClientAwareCommand, ICommand<Result<AuthResponse>>
{
    /// <summary>
    /// Either an email address or the 10-digit mobile number used at
    /// registration. The handler resolves which one it is and looks the
    /// account up accordingly — the field is deliberately not annotated with
    /// [EmailAddress], which would reject mobile-number logins with a 400.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Passphrase { get; init; } = string.Empty;
}
