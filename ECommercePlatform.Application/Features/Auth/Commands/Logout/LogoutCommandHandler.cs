using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<LogoutCommandHandler> logger)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.SessionId is not { } sessionId)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        // Logout is a terminal user intent — even if the browser aborts the
        // HTTP request (navigation, tab close, fetch aborted), the session must
        // still be revoked. Use CancellationToken.None for the critical DB work
        // so a client-side OperationCanceledException doesn't leave the session
        // alive. The passed token is still observed at the start.
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var session = await _sessions.GetOwnedSessionAsync(request.UserId, sessionId, CancellationToken.None);

            if (session is null)
            {
                return Result.Failure(AuthErrors.SessionNotFound);
            }

            session.Revoke(_timeProvider.GetUtcNow(), "logout");
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation(
                "User {UserId} logged out of session {SessionId}.", request.UserId, session.Id);

            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Request was aborted after we already started the DB work with
            // CancellationToken.None — treat as success so the client doesn't
            // retry and the session is still revoked. Logged at Debug, not Error.
            _logger.LogDebug(
                "Logout for user {UserId} was canceled by the client after the session was revoked. Treating as success.",
                request.UserId);
            return Result.Success();
        }
    }
}
