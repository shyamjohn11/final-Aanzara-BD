using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.RevokeSession;

public sealed class RevokeSessionCommandHandler : ICommandHandler<RevokeSessionCommand, Result>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RevokeSessionCommandHandler> _logger;

    public RevokeSessionCommandHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<RevokeSessionCommandHandler> logger)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        // Scoping the lookup by UserId is what stops one user revoking another's
        // session by guessing an id; a foreign id is reported as not found rather
        // than forbidden, so ids stay unenumerable.
        var session = await _sessions.GetOwnedSessionAsync(
            request.UserId, request.TargetSessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        session.Revoke(_timeProvider.GetUtcNow(), "revoked_by_user");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} revoked session {SessionId}.", request.UserId, request.TargetSessionId);

        return Result.Success();
    }
}
