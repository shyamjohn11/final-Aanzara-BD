using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Notifications;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPassphraseHasher _hasher;
    private readonly ISessionManager _sessionManager;
    private readonly IAdminRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository users,
        IPassphraseHasher hasher,
        ISessionManager sessionManager,
        IAdminRepository<Notification> notifications,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<RegisterCommandHandler> logger)
    {
        _users = users;
        _hasher = hasher;
        _sessionManager = sessionManager;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogInformation("Registration rejected: email already in use.");
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyRegistered);
        }

        var now = _timeProvider.GetUtcNow();

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            HashedPassphrase = _hasher.Hash(request.Passphrase),
            // Never taken from the request — self-registration is always Active.
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _users.AddAsync(newUser, cancellationToken);

        var response = await _sessionManager.IssueAsync(newUser, request.Client, cancellationToken);

        NotificationEmitter.Emit(
            _notifications,
            "account",
            $"New customer registered: {newUser.Name}",
            newUser.Email,
            "/admin/users");

        // A unique index on Email is the real guard: the check above races, this
        // commit is what actually enforces it.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user {UserId} registered and signed in.", newUser.UserId);

        return Result.Success(response);
    }
}