using System.Text;
using System.Text.Json;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ECommercePlatform.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Keep claim names exactly as they appear on the wire instead of
                // rewriting them to the legacy WS-* URIs.
                options.MapInboundClaims = false;
                options.SaveToken = false;

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = WriteProblemDetailsOnChallenge,
                    OnForbidden = WriteProblemDetailsOnForbidden,
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            // Lets a client tell "refresh me" apart from "re-login".
                            context.Response.Headers["X-Token-Expired"] = "true";
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ValidateSessionAsync
                };
            });

        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        // Custom provider resolves any [HasPermission] policy name dynamically
        // against the AdminUserRoles/RolePermissions tables at request time,
        // instead of requiring every permission to be pre-registered with
        // AddPolicy. See PermissionAuthorizationPolicyProvider and
        // PermissionAuthorizationHandler.
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Rejects a token whose session has been revoked or has expired, so logout and
    /// "sign out everywhere" take effect immediately instead of at access-token
    /// expiry.
    /// </summary>
    private static async Task ValidateSessionAsync(TokenValidatedContext context)
    {
        var sessionOptions = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<SessionSettings>>().Value;

        if (!sessionOptions.ValidateSessionPerRequest)
        {
            return;
        }

        var sessionIdClaim = context.Principal?.FindFirst(AppClaimTypes.SessionId)?.Value;

        if (!Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            context.Fail("The token carries no usable session identifier.");
            return;
        }

        var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionRepository>();
        var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();

        // Session validation is a short DB lookup that must not be canceled by a browser
        // disconnect (navigating away while the JWT is being validated). Using
        // RequestAborted here makes every aborted image/API request throw
        // OperationCanceledException through JwtBearerHandler as an unhandled 500
        // and triggers VS first-chance breaks. Use None so it always completes.
        try
        {
            if (!await sessions.IsSessionActiveAsync(sessionId, now, CancellationToken.None))
            {
                context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Auth.SessionValidation")
                    .LogInformation("Rejected a token for inactive session {SessionId}.", sessionId);

                context.Fail("The session is no longer active.");
            }
        }
        catch (OperationCanceledException)
        {
            // Fallback in case the DB itself cancels — treat as auth not validated, no 500.
        }
    }

    private static Task WriteProblemDetailsOnChallenge(JwtBearerChallengeContext context)
    {
        // Suppress the default empty-bodied 401 so clients get a consistent
        // ProblemDetails shape from every failure path.
        context.HandleResponse();

        return WriteProblemAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            "Unauthorized",
            "auth.unauthenticated",
            "Authentication is required. Supply a valid Bearer access token.");
    }

    private static Task WriteProblemDetailsOnForbidden(ForbiddenContext context)
        => WriteProblemAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            "Forbidden",
            "auth.forbidden",
            "Your account does not have permission to perform this action.");

    private static async Task WriteProblemAsync(
        HttpContext httpContext, int statusCode, string title, string code, string detail)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var payload = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.io/{statusCode}",
            ["title"] = title,
            ["status"] = statusCode,
            ["code"] = code,
            ["detail"] = detail,
            ["requestId"] = httpContext.TraceIdentifier
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

/// <summary>
/// Fills in the bearer token validation parameters from validated
/// <see cref="JwtOptions"/>, which are only resolvable once DI is built.
/// </summary>
internal sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwt;
    private readonly IHostEnvironment _environment;

    public ConfigureJwtBearerOptions(IOptions<JwtOptions> jwt, IHostEnvironment environment)
    {
        _jwt = jwt.Value;
        _environment = environment;
    }

    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        options.RequireHttpsMetadata = !_environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(_jwt.ClockSkewSeconds),

            // Pin the algorithm so a token cannot talk us into a weaker one.
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    }
}