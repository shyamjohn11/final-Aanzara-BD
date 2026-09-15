namespace ECommercePlatform.Application.Features.Auth.Dtos;

public sealed record AuthResponse
{
    public string TokenType { get; init; } = "Bearer";

    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAtUtc { get; init; }

    /// <summary>
    /// Opaque, single-use token. Each refresh rotates it — the previous value
    /// stops working the moment a new pair is issued.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset RefreshTokenExpiresAtUtc { get; init; }

    public Guid SessionId { get; init; }

    public UserProfileResponse User { get; init; } = new();
}
