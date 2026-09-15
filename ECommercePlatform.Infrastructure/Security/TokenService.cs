using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ECommercePlatform.Infrastructure.Security;

public sealed class TokenService : ITokenService
{
    private const int RefreshTokenBytes = 64;

    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenResult CreateAccessToken(User user, Guid sessionId, IReadOnlyCollection<string> roles)
    {
        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var tokenId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(AppClaimTypes.SessionId, sessionId.ToString())
        };

        // One claim per role — lets ASP.NET's built-in role checks work,
        // and lets the frontend read the role straight off a decoded JWT too.
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _signingCredentials
        };

        return new AccessTokenResult(_handler.CreateToken(descriptor), expiresAt, tokenId);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenBytes));
        var expiresAt = _timeProvider.GetUtcNow().AddDays(_options.RefreshTokenDays);

        return new RefreshTokenResult(raw, HashRefreshToken(raw), expiresAt);
    }

    public string HashRefreshToken(string rawToken)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}

public static class AppClaimTypes
{
    public const string SessionId = "sid";
}