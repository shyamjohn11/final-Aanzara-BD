using ECommercePlatform.Domain.Common;

namespace ECommercePlatform.Domain.Entities;

/// <summary>
/// Account record, matching the USERS table in the schema.
/// </summary>
public class User : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    /// <summary>Public URL of the profile image (null when no avatar uploaded).</summary>
    public string? AvatarUrl { get; set; }

    public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>Free-text self-reported gender (Male/Female/Other/Prefer not to say).</summary>
    public string? Gender { get; set; }

    /// <summary>PBKDF2 hash produced by IPassphraseHasher. Never the raw passphrase.</summary>
    public string HashedPassphrase { get; set; } = string.Empty;

    public string Status { get; set; } = UserStatus.Active;

    public ICollection<UserSession> Sessions { get; set; } = [];
}

/// <summary>The schema stores Status as free text, so these constants keep writers honest.</summary>
public static class UserStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}