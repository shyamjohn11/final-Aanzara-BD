namespace ECommercePlatform.Api.Common;

/// <summary>
/// Gives every request an id, returns it on the response as
/// <c>X-Request-Id</c>, and attaches it to every log line written while handling
/// that request.
/// </summary>
/// <remarks>
/// This is the thread that ties a report back to a root cause: a caller who hits
/// an error quotes the id from the response header (or the <c>requestId</c> field
/// in the error body), and searching the logs for that id returns every line the
/// request produced — controller entry/exit, the dispatched command, EF Core SQL,
/// and the exception with its stack trace.
/// </remarks>
public sealed class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-Id";

    /// <summary>
    /// Longest inbound id we will adopt. Ids end up on every log line for the
    /// request, so an unbounded one would let a caller inflate the log volume.
    /// </summary>
    private const int MaxInboundLength = 64;

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = ResolveRequestId(context);

        // Align the framework's trace identifier so ProblemDetails responses and
        // log lines quote the same value without being told about it.
        context.TraceIdentifier = requestId;

        // Set on the way out rather than now, so it survives the response being
        // cleared and rewritten — which is exactly what the exception handler does
        // when turning a failure into ProblemDetails.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = requestId;
            return Task.CompletedTask;
        });

        // RequestIdEnricher puts this id on every log event for the request; it
        // reads TraceIdentifier, which is why setting it above is what matters.
        await _next(context);
    }

    /// <summary>
    /// Adopts a caller-supplied id when it is safe to, so an id set by a gateway or
    /// an upstream service carries through; otherwise mints a fresh one.
    /// </summary>
    private static string ResolveRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values)
            && IsSafeRequestId(values.ToString()))
        {
            return values.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Accepts only an unambiguous token. Rejecting anything else keeps
    /// caller-controlled text out of log files and response headers, where a
    /// newline or control character could forge a log line or split a header.
    /// </summary>
    private static bool IsSafeRequestId(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > MaxInboundLength)
        {
            return false;
        }

        foreach (var c in candidate)
        {
            var allowed = char.IsAsciiLetterOrDigit(c) || c is '-' or '_';

            if (!allowed)
            {
                return false;
            }
        }

        return true;
    }
}
