using System.ComponentModel.DataAnnotations.Schema;

namespace ECommercePlatform.Domain.Entities;

/// <summary>
/// A single logged-in session (one device / one browser). The refresh token is
/// stored only as a SHA-256 hash, so a database leak does not hand out usable
/// tokens. Rotation links sessions together via <see cref="ReplacedBySessionId"/>,
/// which is what lets us detect refresh-token replay.
/// </summary>
public class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Base64 SHA-256 hash of the refresh token. Never the raw token.</summary>
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public string? RevokedReason { get; set; }

    /// <summary>Set when this session was rotated into a new one during refresh.</summary>
    public Guid? ReplacedBySessionId { get; set; }

    public string? CreatedByIp { get; set; }

    public string? UserAgent { get; set; }

    [NotMapped]
    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAtUtc;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public void Revoke(DateTimeOffset now, string reason, Guid? replacedBy = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = now;
        RevokedReason = reason;
        ReplacedBySessionId = replacedBy;
    }
}
