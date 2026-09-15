using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

public sealed record AccessTokenResult(string Value, DateTimeOffset ExpiresAtUtc, string TokenId);

public sealed record RefreshTokenResult(string Value, string Hash, DateTimeOffset ExpiresAtUtc);

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(User user, Guid sessionId, IReadOnlyCollection<string> roles);

    RefreshTokenResult CreateRefreshToken();

    /// <summary>Hashes a raw refresh token for comparison against stored hashes.</summary>
    string HashRefreshToken(string rawToken);
}