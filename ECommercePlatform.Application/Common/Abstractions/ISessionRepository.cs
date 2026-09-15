using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public interface ISessionRepository
{
    /// <summary>Finds a session by refresh-token hash, including its user.</summary>
    Task<UserSession?> GetByRefreshTokenHashAsync(string hash, CancellationToken cancellationToken);

    /// <summary>Scoped by user id so one caller cannot reach another's session.</summary>
    Task<UserSession?> GetOwnedSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSession>> GetActiveAsync(
        Guid userId, DateTimeOffset now, CancellationToken cancellationToken);

    /// <summary>True while the session behind an access token is still usable.</summary>
    Task<bool> IsSessionActiveAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken);

    void Add(UserSession session);

    /// <summary>Bulk-deletes sessions expired before the cutoff. Returns rows removed.</summary>
    Task<int> PurgeExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);
}
