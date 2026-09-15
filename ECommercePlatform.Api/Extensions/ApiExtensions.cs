using Serilog.Core;
using System.Globalization;
using System.Threading.RateLimiting;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Api.Configuration;
using ECommercePlatform.Application.Common.Messaging;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommercePlatform.Api.Extensions;

public static class ApiExtensions
{
    /// <summary>Rate-limiting policy guarding the credential endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>
    /// Only what the Api layer itself owns. Options for the JWT signing key and
    /// for sessions belong to Infrastructure and Application respectively, and are
    /// registered by their own AddInfrastructure / AddApplication roots.
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        // The only ICurrentUser implementation that knows about HTTP.
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Picked up by Serilog through ReadFrom.Services, so every log event
        // carries the same request id we return in the X-Request-Id header.
        services.AddSingleton<ILogEventEnricher, RequestIdEnricher>();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddApiBehavior(this IServiceCollection services)
    {
        services.AddControllers();

        // Every error response — framework or ours — comes back as ProblemDetails.
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??=
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions["requestId"] = context.HttpContext.TraceIdentifier;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Model-validation failures should carry the same envelope and requestId as
        // everything else.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Type = "https://httpstatuses.io/400",
                    Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
                };

                problem.Extensions["code"] = "request.validation_failed";
                problem.Extensions["requestId"] = context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }

    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>()?.AllowedOrigins ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                if (origins.Length == 0)
                {
                    // No configured origins means no cross-origin browser access.
                    // Never widen this to AllowAnyOrigin with credentials.
                    return;
                }

                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders(RequestIdMiddleware.HeaderName, "X-Token-Expired")
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Tight budget on the credential endpoints: enough for a human getting
            // their password wrong a few times, far too little to brute-force with.
            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: PartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            // Broad backstop for everything else.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: PartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiter")
                    .LogWarning(
                        "Rate limit rejected {Method} {Path} from {IpAddress}.",
                        context.HttpContext.Request.Method,
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);

                var problemDetailsService = context.HttpContext.RequestServices
                    .GetRequiredService<IProblemDetailsService>();

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests",
                    Type = "https://httpstatuses.io/429",
                    Detail = "Too many requests. Slow down and try again shortly."
                };

                problem.Extensions["code"] = "request.rate_limited";

                await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = problem
                });
            };
        });

        return services;
    }

    /// <summary>
    /// Partitions by authenticated user where possible so one user behind a shared
    /// NAT cannot exhaust everyone else's budget; falls back to remote IP.
    /// </summary>
    private static string PartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst("sub")?.Value
            : null;

        return userId
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
