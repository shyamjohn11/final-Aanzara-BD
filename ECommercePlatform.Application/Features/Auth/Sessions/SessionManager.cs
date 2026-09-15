using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Application.Features.Auth.Sessions;

/// <summary>
/// Owns the session lifecycle shared by several handlers. Nothing here calls
/// SaveChanges — the handler keeps the transaction boundary and commits once.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Starts a session and mints its token pair, enforcing the per-user session
    /// cap. The new row is staged but not committed.
    /// </summary>
    Task<AuthResponse> IssueAsync(User user, ClientInfo client, CancellationToken cancellationToken);

    Task<int> RevokeAllAsync(Guid userId, string reason, CancellationToken cancellationToken);

    Task<int> RevokeAllExceptAsync(
        Guid userId, Guid? exceptSessionId, string reason, CancellationToken cancellationToken);

    UserProfileResponse MapProfile(User user, IReadOnlyCollection<string> roles);
}

public sealed class SessionManager : ISessionManager
{
    private readonly ISessionRepository _sessions;
    private readonly ITokenService _tokenService;
    private readonly IRoleService _roleService;
    private readonly TimeProvider _timeProvider;
    private readonly SessionSettings _options;
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(
        ISessionRepository sessions,
        ITokenService tokenService,
        IRoleService roleService,
        TimeProvider timeProvider,
        IOptions<SessionSettings> options,
        ILogger<SessionManager> logger)
    {
        _sessions = sessions;
        _tokenService = tokenService;
        _roleService = roleService;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> IssueAsync(
        User user, ClientInfo client, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var sessionId = Guid.NewGuid();

        // Resolved once per issue and reused for both the JWT claims and the
        // response body, so the token and the profile never disagree.
        var roles = await _roleService.GetRolesForUserAsync(user.UserId, cancellationToken);

        var refreshToken = _tokenService.CreateRefreshToken();
        var accessToken = _tokenService.CreateAccessToken(user, sessionId, roles);

        await EnforceSessionLimitAsync(user.UserId, now, cancellationToken);

        _sessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = user.UserId,
            RefreshTokenHash = refreshToken.Hash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            LastUsedAtUtc = now,
            CreatedByIp = client.IpAddress,
            UserAgent = client.UserAgent
        });

        return new AuthResponse
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken.Value,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            SessionId = sessionId,
            User = MapProfile(user, roles)
        };
    }

    public Task<int> RevokeAllAsync(Guid userId, string reason, CancellationToken cancellationToken)
        => RevokeAllExceptAsync(userId, exceptSessionId: null, reason, cancellationToken);

    public async Task<int> RevokeAllExceptAsync(
        Guid userId, Guid? exceptSessionId, string reason, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var sessions = await _sessions.GetActiveAsync(userId, now, cancellationToken);

        var count = 0;

        foreach (var session in sessions.Where(s => s.Id != exceptSessionId))
        {
            session.Revoke(now, reason);
            count++;
        }

        return count;
    }

    public UserProfileResponse MapProfile(User user, IReadOnlyCollection<string> roles) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        Phone = user.Phone,
        Status = user.Status,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        Roles = roles
    };

    /// <summary>Retires the least recently used sessions so they cannot accumulate without bound.</summary>
    private async Task EnforceSessionLimitAsync(
        Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var limit = _options.MaxActiveSessionsPerUser;

        if (limit <= 0)
        {
            return;
        }

        var active = (await _sessions.GetActiveAsync(userId, now, cancellationToken))
            .OrderBy(s => s.LastUsedAtUtc ?? s.CreatedAtUtc)
            .ToList();

        // We are about to add one more session, so make room for it.
        var excess = active.Count - limit + 1;

        for (var i = 0; i < excess && i < active.Count; i++)
        {
            active[i].Revoke(now, "session_limit_exceeded");

            _logger.LogInformation(
                "Session {SessionId} for user {UserId} retired: active session limit ({Limit}) reached.",
                active[i].Id, userId, limit);
        }
    }
}

/// <summary>Session tuning, bound from the "Session" configuration section.</summary>
public sealed class SessionSettings
{
    public const string SectionName = "Session";

    /// <summary>Maximum concurrent active sessions per user. 0 disables the cap.</summary>
    public int MaxActiveSessionsPerUser { get; set; } = 5;

    /// <summary>
    /// When true, every authenticated request re-checks that the token's session
    /// is still active, so logout takes effect immediately rather than at
    /// access-token expiry.
    /// </summary>
    public bool ValidateSessionPerRequest { get; set; } = true;
}