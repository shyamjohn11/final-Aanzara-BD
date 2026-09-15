using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.ChangePasswordWithOtp;

public sealed class ChangePasswordWithOtpCommandHandler : ICommandHandler<ChangePasswordWithOtpCommand, Result>
{
    private const string OtpPurpose = "PASSWORD_RESET";

    private readonly IUserRepository _users;
    private readonly IOtpStore _otpStore;
    private readonly IPassphraseHasher _hasher;
    private readonly ISessionManager _sessionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordWithOtpCommandHandler> _logger;

    public ChangePasswordWithOtpCommandHandler(
        IUserRepository users,
        IOtpStore otpStore,
        IPassphraseHasher hasher,
        ISessionManager sessionManager,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordWithOtpCommandHandler> logger)
    {
        _users = users;
        _otpStore = otpStore;
        _hasher = hasher;
        _sessionManager = sessionManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ChangePasswordWithOtpCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var otp = request.Otp.Trim();

        // 1. Verify and atomically consume the OTP (single-use).
        var isOtpValid = await _otpStore.ValidateAndConsumeOtpAsync(
            email, OtpPurpose, otp, cancellationToken);

        if (!isOtpValid)
        {
            _logger.LogWarning("Password reset rejected for {Email}: OTP is invalid or expired.", email);
            return Result.Failure(AuthErrors.InvalidOrExpiredOtp);
        }

        // 2. Fetch the user.
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Password reset verified OTP but user {Email} was not found.", email);
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // 3. Hash the new passphrase and update the user.
        user.HashedPassphrase = _hasher.Hash(request.NewPassphrase);
        await _users.UpdateAsync(user, cancellationToken);

        // 4. Invalidate all active sessions for this user across all devices.
        var revokedSessions = await _sessionManager.RevokeAllAsync(
            user.UserId, "passphrase_reset_via_otp", cancellationToken);

        // 5. Commit the user update and session revocations in a single transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} ({Email}) successfully changed their passphrase via email OTP; {Count} session(s) revoked.",
            user.UserId, email, revokedSessions);

        return Result.Success();
    }
}
