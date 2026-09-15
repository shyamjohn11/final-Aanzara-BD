using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Auth;
using ECommercePlatform.Application.Features.Auth.Commands.ChangePassphrase;
using ECommercePlatform.Application.Features.Auth.Commands.ChangePasswordWithOtp;
using ECommercePlatform.Application.Features.Auth.Commands.Login;
using ECommercePlatform.Application.Features.Auth.Commands.Logout;
using ECommercePlatform.Application.Features.Auth.Commands.LogoutEverywhere;
using ECommercePlatform.Application.Features.Auth.Commands.RefreshToken;
using ECommercePlatform.Application.Features.Auth.Commands.Register;
using ECommercePlatform.Application.Features.Auth.Commands.RevokeSession;
using ECommercePlatform.Application.Features.Auth.Commands.SendPasswordOtp;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Application.Features.Auth.Queries.GetActiveSessions;
using ECommercePlatform.Application.Features.Auth.Queries.GetProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Binds HTTP to messages and nothing else: no business logic, no data access.
/// </summary>
/// <remarks>
/// Each action logs on entry and, from a finally block, on exit — so the pair is
/// written whether the action returns a payload, returns a failure, or throws.
/// There is no catch: an exception propagates untouched to
/// <see cref="GlobalExceptionHandler"/>, which logs it with its stack trace and
/// turns it into a ProblemDetails response.
/// </remarks>
[Authorize]
[Route("api/v1/auth")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class AuthController : ApiControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ISender sender,
        ICurrentUser currentUser,
        ILogger<AuthController> logger)
        : base(sender)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Creates an account and signs it in. Returns the new token pair.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Register action started.");

        try
        {
            var result = await Sender.Send(command with { Client = Client }, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Register action finished.");
        }
    }

    /// <summary>Exchanges credentials for an access token and a refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login action started.");

        try
        {
            var result = await Sender.Send(command with { Client = Client }, cancellationToken);
            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Login action finished.");
        }
    }

    /// <summary>
    /// Exchanges a refresh token for a fresh token pair. The submitted refresh
    /// token is single-use — it is revoked as soon as the replacement is issued.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh action started.");

        try
        {
            var result = await Sender.Send(command with { Client = Client }, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Refresh action finished.");
        }
    }

    /// <summary>Ends the session the calling access token belongs to. No payload.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            var result = await Sender.Send(
                new LogoutCommand { UserId = userId, SessionId = _currentUser.SessionId },
                cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Logout action finished.");
        }
    }

    /// <summary>Ends every active session for the current user, on all devices.</summary>
    [HttpPost("logout-all")]
    [ProducesResponseType(typeof(LogoutEverywhereResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LogoutEverywhereResponse>> LogoutEverywhere(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("LogoutEverywhere action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            var result = await Sender.Send(
                new LogoutEverywhereCommand { UserId = userId, SessionId = _currentUser.SessionId },
                cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return Ok(new LogoutEverywhereResponse { RevokedSessions = result.Value });
        }
        finally
        {
            _logger.LogInformation("LogoutEverywhere action finished.");
        }
    }

    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Me action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            var result = await Sender.Send(new GetProfileQuery(userId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Me action finished.");
        }
    }

    /// <summary>Lists the current user's active sessions, flagging the current one.</summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyCollection<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SessionResponse>>> Sessions(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sessions action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            var result = await Sender.Send(
                new GetActiveSessionsQuery(userId, _currentUser.SessionId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Sessions action finished.");
        }
    }

    /// <summary>Revokes one of the current user's sessions, e.g. a lost device.</summary>
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("RevokeSession action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            var result = await Sender.Send(
                new RevokeSessionCommand
                {
                    UserId = userId,
                    SessionId = _currentUser.SessionId,
                    TargetSessionId = sessionId
                },
                cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("RevokeSession action finished.");
        }
    }

    /// <summary>
    /// Changes the passphrase and signs the user out everywhere except the device
    /// that made the change.
    /// </summary>
    [HttpPost("change-passphrase")]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePassphrase(
        [FromBody] ChangePassphraseCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ChangePassphrase action started.");

        try
        {
            if (_currentUser.UserId is not { } userId)
            {
                return ToProblem(AuthErrors.InvalidCredentials);
            }

            // The bound command carries only body fields; identity is overwritten
            // here from the token so a caller cannot inject someone else's id.
            var result = await Sender.Send(
                command with { UserId = userId, SessionId = _currentUser.SessionId },
                cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("ChangePassphrase action finished.");
        }
    }

    /// <summary>Generates and sends a 6-digit OTP to the specified email for password change or reset.</summary>
    [HttpPost("password/send-otp")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SendPasswordOtp(
        [FromBody] SendPasswordOtpCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SendPasswordOtp action started.");

        try
        {
            var result = await Sender.Send(command, cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("SendPasswordOtp action finished.");
        }
    }

    /// <summary>Verifies the emailed OTP and changes the user's passphrase, revoking all active sessions.</summary>
    [HttpPost("password/change-with-otp")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiExtensions.AuthRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangePasswordWithOtp(
        [FromBody] ChangePasswordWithOtpCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ChangePasswordWithOtp action started.");

        try
        {
            var result = await Sender.Send(command, cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("ChangePasswordWithOtp action finished.");
        }
    }
}

