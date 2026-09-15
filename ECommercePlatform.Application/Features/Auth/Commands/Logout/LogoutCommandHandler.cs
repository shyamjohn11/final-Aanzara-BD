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

        var session = await _sessions.GetOwnedSessionAsync(request.UserId, sessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        session.Revoke(_timeProvider.GetUtcNow(), "logout");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged out of session {SessionId}.", request.UserId, session.Id);

        return Result.Success();
    }
}
