using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Queries.GetActiveSessions;

public sealed class GetActiveSessionsQueryHandler
    : IQueryHandler<GetActiveSessionsQuery, Result<IReadOnlyCollection<SessionResponse>>>
{
    private readonly ISessionRepository _sessions;
    private readonly TimeProvider _timeProvider;

    public GetActiveSessionsQueryHandler(ISessionRepository sessions, TimeProvider timeProvider)
    {
        _sessions = sessions;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyCollection<SessionResponse>>> Handle(
        GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _sessions.GetActiveAsync(
            request.UserId, _timeProvider.GetUtcNow(), cancellationToken);

        var response = sessions
            .OrderByDescending(s => s.LastUsedAtUtc ?? s.CreatedAtUtc)
            .Select(s => new SessionResponse
            {
                Id = s.Id,
                CreatedAtUtc = s.CreatedAtUtc,
                ExpiresAtUtc = s.ExpiresAtUtc,
                LastUsedAtUtc = s.LastUsedAtUtc,
                IpAddress = s.CreatedByIp,
                UserAgent = s.UserAgent,
                IsCurrent = request.CurrentSessionId is not null && s.Id == request.CurrentSessionId
            })
            .ToArray();

        return Result.Success<IReadOnlyCollection<SessionResponse>>(response);
    }
}
