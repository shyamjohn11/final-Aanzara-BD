using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth;

/// <summary>
/// Centralised auth failures. Note that bad-email and bad-passphrase both return
/// <see cref="InvalidCredentials"/> — distinguishing them would let an attacker
/// enumerate registered accounts.
/// </summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "Email or passphrase is incorrect.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("auth.email_already_registered", "An account with this email address already exists.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "The refresh token is invalid, expired, or already used.");

    public static readonly Error UserNotFound =
        Error.NotFound("auth.user_not_found", "The user could not be found.");

    public static readonly Error SessionNotFound =
        Error.NotFound("auth.session_not_found", "The session could not be found.");

    public static readonly Error IncorrectCurrentPassphrase =
        Error.Validation("auth.incorrect_current_passphrase", "The current passphrase is incorrect.");

    public static readonly Error InvalidOrExpiredOtp =
        Error.Validation("auth.invalid_otp", "The provided OTP is invalid, expired, or has already been used.");
}

