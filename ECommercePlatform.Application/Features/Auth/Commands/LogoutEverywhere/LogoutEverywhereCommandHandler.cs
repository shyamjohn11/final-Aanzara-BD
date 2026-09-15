using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.LogoutEverywhere;

public sealed class LogoutEverywhereCommandHandler
    : ICommandHandler<LogoutEverywhereCommand, Result<int>>
{
    private readonly ISessionManager _sessionManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutEverywhereCommandHandler> _logger;

    public LogoutEverywhereCommandHandler(
        ISessionManager sessionManager,
        IUnitOfWork unitOfWork,
        ILogger<LogoutEverywhereCommandHandler> logger)
    {
        _sessionManager = sessionManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        LogoutEverywhereCommand request, CancellationToken cancellationToken)
    {
        var revoked = await _sessionManager.RevokeAllAsync(
            request.UserId, "logout_all_devices", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged out of {Count} session(s).", request.UserId, revoked);

        return Result.Success(revoked);
    }
}
