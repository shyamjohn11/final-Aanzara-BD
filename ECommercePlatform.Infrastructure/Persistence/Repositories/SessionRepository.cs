using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext _db;

    public SessionRepository(ApplicationDbContext db) => _db = db;

    public Task<UserSession?> GetByRefreshTokenHashAsync(string hash, CancellationToken cancellationToken)
        => _db.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == hash, cancellationToken);

    public Task<UserSession?> GetOwnedSessionAsync(
        Guid userId, Guid sessionId, CancellationToken cancellationToken)
        // Filtering on UserId as well as the id is the authorization check: a
        // session belonging to someone else simply is not found.
        => _db.UserSessions.FirstOrDefaultAsync(
            s => s.Id == sessionId && s.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetActiveAsync(
        Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
        => await _db.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null && s.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

    public Task<bool> IsSessionActiveAsync(
        Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken)
        // Runs on every authenticated request, so it stays a no-tracking key seek.
        => _db.UserSessions
            .AsNoTracking()
            .AnyAsync(
                s => s.Id == sessionId && s.RevokedAtUtc == null && s.ExpiresAtUtc > now,
                cancellationToken);

    public void Add(UserSession session) => _db.UserSessions.Add(session);

    public Task<int> PurgeExpiredAsync(DateTimeOffset cutoff, CancellationToken cancellationToken)
        // Set-based delete: never load rows we only intend to discard.
        => _db.UserSessions
            .Where(s => s.ExpiresAtUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
}
