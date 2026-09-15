using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Common.Messaging.Behaviors;

/// <summary>
/// One log line per dispatched message, carrying the outcome and the elapsed time.
/// This is the layer at which "what did the system actually do" is legible —
/// Serilog's request log says an HTTP call happened, this says which command ran.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Anything slower than this gets promoted to a warning.</summary>
    private const int SlowRequestMilliseconds = 500;

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var kind = request is ICommand<TResponse> ? "Command" : "Query";

        // Scoped properties attach to every log line the handler itself writes,
        // so handler logs are correlated without each one repeating the context.
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["RequestName"] = name,
            ["RequestKind"] = kind
        });

        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await next(cancellationToken);
            var elapsed = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

            // A failed Result is an expected outcome, not an error: log it at
            // Information with its code so it stays searchable without alerting.
            if (response is Result { IsFailure: true } failure)
            {
                _logger.LogInformation(
                    "{Kind} {Name} rejected in {Elapsed:0.0} ms: {ErrorCode}",
                    kind, name, elapsed, failure.Error!.Code);
            }
            else if (elapsed > SlowRequestMilliseconds)
            {
                _logger.LogWarning(
                    "{Kind} {Name} completed in {Elapsed:0.0} ms (slower than {Threshold} ms).",
                    kind, name, elapsed, SlowRequestMilliseconds);
            }
            else
            {
                _logger.LogInformation("{Kind} {Name} completed in {Elapsed:0.0} ms.", kind, name, elapsed);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Kind} {Name} threw after {Elapsed:0.0} ms.",
                kind, name, Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

            throw;
        }
    }
}
