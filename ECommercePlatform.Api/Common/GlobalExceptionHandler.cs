using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Common;

/// <summary>
/// Converts any unhandled exception into an RFC 7807 ProblemDetails response.
/// Details are deliberately generic: the stack trace goes to the log, never to
/// the caller, and the requestId ties the two together.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _environment = environment;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The client went away (browser navigated, Swagger UI cancelled a
        // timed-out /openapi fetch, timeout). There is nobody to answer and
        // nothing is broken server-side, so stay quiet instead of logging a
        // 500 for every aborted request.
        if (exception is OperationCanceledException
            || httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Request {Method} {Path} was canceled by the client.",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return true;
        }

        var requestId = httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path}. RequestId: {RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            requestId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Detail = _environment.IsDevelopment()
                ? exception.ToString()
                : "The request could not be completed. Quote the requestId when reporting this."
        };

        problemDetails.Extensions["requestId"] = requestId;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
