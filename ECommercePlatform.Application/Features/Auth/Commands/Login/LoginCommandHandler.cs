using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPassphraseHasher _hasher;
    private readonly ISessionManager _sessionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository users,
        IPassphraseHasher hasher,
        ISessionManager sessionManager,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
    {
        _users = users;
        _hasher = hasher;
        _sessionManager = sessionManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        // The single login field can be an email or a 10-digit mobile number;
        // the repository resolves whichever it is.
        var user = await _users.GetByEmailOrPhoneAsync(request.Email.Trim(), cancellationToken);

        if (user is null)
        {
            // Burn comparable CPU to a real verification so response timing does
            // not reveal whether the address is registered.
            _hasher.Verify(DummyHash.Value, request.Passphrase);

            _logger.LogInformation(
                "Login failed for an unknown email address or phone from {IpAddress}.", request.Client.IpAddress);

            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        var verification = _hasher.Verify(user.HashedPassphrase, request.Passphrase);

        if (verification == PassphraseVerificationResult.Failed)
        {
            _logger.LogInformation("Login failed for user {UserId}: incorrect passphrase.", user.UserId);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        if (verification == PassphraseVerificationResult.SuccessRehashNeeded)
        {
            // Opportunistic upgrade: the passphrase is correct, so this is the one
            // moment we hold the plaintext and can raise the stored work factor.
            user.HashedPassphrase = _hasher.Hash(request.Passphrase);
            await _users.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Upgraded the stored hash for user {UserId}.", user.UserId);
        }

        var response = await _sessionManager.IssueAsync(user, request.Client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} signed in. Session {SessionId}.", user.UserId, response.SessionId);

        return Result.Success(response);
    }

    /// <summary>
    /// A real hash over a throwaway value, used only to make an unknown-email
    /// login cost roughly the same as a known-email one.
    /// </summary>
    private static class DummyHash
    {
        // Precomputed rather than hashed at start-up so no plaintext is held.
        public static readonly string Value =
            "AQAAAAIAAYagAAAAEFlAfOmT5rn17QmNohyRcuvI7Z+yWEmyZ6fpsUsfeTQavmcmpzL2WcuuOrVRclgYtg==";
    }
}
