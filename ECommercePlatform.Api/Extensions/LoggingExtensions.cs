using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Security;
using Serilog;
using Serilog.Events;

namespace ECommercePlatform.Api.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Replaces the default logging stack with Serilog, configured from the
    /// "Serilog" section so sinks and levels are changeable without a rebuild.
    /// </summary>
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ECommercePlatform")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

        return builder;
    }

    /// <summary>
    /// One summary log line per request, enriched with the caller's identity. This
    /// replaces the framework's chatty multi-line-per-request output.
    /// </summary>
    public static WebApplication UseApplicationRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = static (httpContext, _, exception) =>
            {
                // Health probes fire constantly; keep them out of the normal stream.
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                {
                    return LogEventLevel.Verbose;
                }

                if (exception is not null || httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("Scheme", httpContext.Request.Scheme);

                // Query strings can carry identifiers worth correlating, but never
                // log the request body — that is where credentials live.
                if (httpContext.Request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                }

                var currentUser = httpContext.RequestServices.GetService<ICurrentUser>();

                if (currentUser?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", currentUser.UserId);
                    diagnosticContext.Set("SessionId", currentUser.SessionId);
                }
            };
        });

        return app;
    }

    public static WebApplication UseRequestId(this WebApplication app)
    {
        app.UseMiddleware<RequestIdMiddleware>();
        return app;
    }
}
