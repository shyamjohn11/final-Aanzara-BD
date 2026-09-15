using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.ChangePassphrase;

public sealed class ChangePassphraseCommandHandler : ICommandHandler<ChangePassphraseCommand, Result>
{
    private readonly IUserRepository _users;
    private readonly IPassphraseHasher _hasher;
    private readonly ISessionManager _sessionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePassphraseCommandHandler> _logger;

    public ChangePassphraseCommandHandler(
        IUserRepository users,
        IPassphraseHasher hasher,
        ISessionManager sessionManager,
        IUnitOfWork unitOfWork,
        ILogger<ChangePassphraseCommandHandler> logger)
    {
        _users = users;
        _hasher = hasher;
        _sessionManager = sessionManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ChangePassphraseCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        if (_hasher.Verify(user.HashedPassphrase, request.CurrentPassphrase)
            == PassphraseVerificationResult.Failed)
        {
            _logger.LogInformation("Passphrase change failed for user {UserId}.", request.UserId);
            return Result.Failure(AuthErrors.IncorrectCurrentPassphrase);
        }

        user.HashedPassphrase = _hasher.Hash(request.NewPassphrase);
        await _users.UpdateAsync(user, cancellationToken);

        // A passphrase change should evict anyone else holding a session, while
        // leaving the caller signed in on the device they changed it from.
        var revoked = await _sessionManager.RevokeAllExceptAsync(
            request.UserId, request.SessionId, "passphrase_changed", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} changed their passphrase; {Count} other session(s) revoked.",
            request.UserId, revoked);

        return Result.Success();
    }
}
