using Serilog.Core;
using Serilog.Events;

namespace ECommercePlatform.Api.Common;

/// <summary>
/// Stamps our request id onto every log event as <c>RequestId</c>.
/// </summary>
/// <remarks>
/// ASP.NET Core's hosting layer already publishes a logging scope property called
/// <c>RequestId</c>, holding Kestrel's connection-scoped identifier (for example
/// <c>0HNO4CSD57BKN:00000001</c>). That scope is created before any middleware
/// runs, so it captures the original <c>TraceIdentifier</c> and would otherwise
/// shadow the id we hand back in the <c>X-Request-Id</c> header — searching the
/// logs for an id from a response would find nothing.
/// <para>
/// Enrichers are applied after scope properties, so <c>AddOrUpdateProperty</c>
/// here overwrites the framework's value with ours and the two agree.
/// </para>
/// </remarks>
public sealed class RequestIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestIdEnricher(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Null outside a request — background jobs and start-up simply have no
        // request id, and keep whatever the framework supplied.
        if (_httpContextAccessor.HttpContext is not { } httpContext)
        {
            return;
        }

        logEvent.AddOrUpdateProperty(
            new LogEventProperty("RequestId", new ScalarValue(httpContext.TraceIdentifier)));
    }
}
