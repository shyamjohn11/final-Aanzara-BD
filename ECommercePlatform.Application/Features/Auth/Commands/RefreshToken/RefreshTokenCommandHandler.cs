using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly ISessionRepository _sessions;
    private readonly ITokenService _tokenService;
    private readonly ISessionManager _sessionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        ISessionRepository sessions,
        ITokenService tokenService,
        ISessionManager sessionManager,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _sessions = sessions;
        _tokenService = tokenService;
        _sessionManager = sessionManager;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashRefreshToken(request.RefreshToken);
        var now = _timeProvider.GetUtcNow();

        var session = await _sessions.GetByRefreshTokenHashAsync(hash, cancellationToken);

        if (session is null)
        {
            _logger.LogWarning(
                "Refresh rejected: unknown refresh token presented from {IpAddress}.",
                request.Client.IpAddress);

            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (session.IsRevoked)
        {
            // An already-rotated token came back. Either it was stolen or a client
            // replayed it, and we cannot tell which, so we invalidate every session
            // for the user and force a fresh login.
            _logger.LogWarning(
                "Refresh token replay detected for user {UserId} on session {SessionId} from {IpAddress}. Revoking all sessions.",
                session.UserId, session.Id, request.Client.IpAddress);

            await _sessionManager.RevokeAllAsync(
                session.UserId, "refresh_token_replay_detected", cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (session.IsExpired(now))
        {
            _logger.LogInformation("Refresh rejected: session {SessionId} has expired.", session.Id);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        var response = await _sessionManager.IssueAsync(session.User, request.Client, cancellationToken);

        // Rotation: the presented token dies the moment its replacement is issued.
        session.Revoke(now, "rotated", response.SessionId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Session {OldSessionId} rotated into {NewSessionId} for user {UserId}.",
            session.Id, response.SessionId, session.UserId);

        return Result.Success(response);
    }
}
