using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Queries.GetActiveSessions;

/// <summary>
/// The caller's active sessions. CurrentSessionId is only used to flag which row
/// is the caller's own device.
/// </summary>
public sealed record GetActiveSessionsQuery(Guid UserId, Guid? CurrentSessionId)
    : IQuery<Result<IReadOnlyCollection<SessionResponse>>>;
