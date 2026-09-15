using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.RevokeSession;

/// <summary>Revokes one of the caller's own sessions, e.g. a lost device.</summary>
public sealed record RevokeSessionCommand : AuthenticatedCommand, ICommand<Result>
{
    /// <summary>Comes from the route, not the body.</summary>
    public Guid TargetSessionId { get; init; }
}
