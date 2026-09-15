namespace ECommercePlatform.Application.Features.Auth.Dtos;

/// <summary>
/// Result of signing out of every device. A named type rather than an anonymous
/// object so the endpoint's response shape appears in its own signature and in
/// the OpenAPI document.
/// </summary>
public sealed record LogoutEverywhereResponse
{
    /// <summary>How many active sessions were revoked by the call.</summary>
    public int RevokedSessions { get; init; }
}
