using System.ComponentModel.DataAnnotations;

namespace ECommercePlatform.Infrastructure.Security;

/// <summary>
/// Bound from the "Jwt" section and validated at startup, so a missing or weak
/// signing key fails the host immediately rather than at the first login.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// HMAC-SHA256 signing key. Must never live in appsettings.json — supply it
    /// via user-secrets in development and a secret store elsewhere.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters (256 bits) for HMAC-SHA256.")]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; set; } = 14;

    [Range(0, 300)]
    public int ClockSkewSeconds { get; set; } = 30;
}
