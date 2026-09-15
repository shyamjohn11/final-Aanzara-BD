namespace ECommercePlatform.Application.Common.Security;

/// <summary>Caller fingerprint recorded against each session for auditing.</summary>
/// <remarks>
/// A plain value with no knowledge of HTTP: mapping an incoming request onto it is
/// the Api layer's job, which is what keeps Application transport-agnostic.
/// </remarks>
public sealed record ClientInfo(string? IpAddress, string? UserAgent)
{
    /// <summary>Used when a request did not originate from a remote caller.</summary>
    public static readonly ClientInfo Unknown = new(null, null);

    public static ClientInfo Create(string? ipAddress, string? userAgent) => new(
        Truncate(ipAddress, 64),
        Truncate(string.IsNullOrWhiteSpace(userAgent) ? null : userAgent, 512));

    private static string? Truncate(string? value, int maxLength)
        => value is null || value.Length <= maxLength ? value : value[..maxLength];
}
