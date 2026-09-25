using ECommercePlatform.Application.Features.Auth.Dtos;

namespace ECommercePlatform.Api.Extensions;

/// <summary>
/// HttpOnly auth cookies. Refresh token is never readable from JavaScript.
/// Role/session cookies are HttpOnly so the browser cannot be tricked into
/// elevating them from script; the edge proxy reads them from the request only.
/// </summary>
public static class AuthCookies
{
    public const string AccessCookieName = "aanzara_at";
    public const string RefreshCookieName = "aanzara_rt";
    public const string SessionCookieName = "aanzara_session";
    public const string RoleCookieName = "aanzara_role";

    public static void SetAuthCookies(HttpContext context, AuthResponse auth)
    {
        var isProd = !context.RequestServices
            .GetService<IHostEnvironment>()?.IsDevelopment() ?? true;

        var accessExpiry = auth.AccessTokenExpiresAtUtc;
        var refreshExpiry = auth.RefreshTokenExpiresAtUtc;
        var role = ResolvePrimaryRole(auth.User.Roles);

        Append(context, AccessCookieName, auth.AccessToken, accessExpiry, httpOnly: true);
        Append(context, RefreshCookieName, auth.RefreshToken, refreshExpiry, httpOnly: true);
        Append(context, SessionCookieName, "1", refreshExpiry, httpOnly: true);
        Append(context, RoleCookieName, role, refreshExpiry, httpOnly: true);

        _ = isProd; // Secure flag applied below uniformly; prod still forces HTTPS via HSTS.
    }

    public static void ClearAuthCookies(HttpContext context)
    {
        foreach (var name in new[]
                 {
                     AccessCookieName, RefreshCookieName, SessionCookieName, RoleCookieName,
                     "aanzara_session", "aanzara_role"
                 })
        {
            context.Response.Cookies.Delete(name, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }
    }

    private static void Append(
        HttpContext context, string name, string value, DateTimeOffset expires, bool httpOnly)
    {
        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            Path = "/",
            HttpOnly = httpOnly,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expires
        });
    }

    private static string ResolvePrimaryRole(IReadOnlyCollection<string> roles)
    {
        if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)) return "admin";
        if (roles.Contains("Agent", StringComparer.OrdinalIgnoreCase)) return "agent";
        return "customer";
    }
}
