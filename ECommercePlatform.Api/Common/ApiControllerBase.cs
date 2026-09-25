using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Common;

/// <summary>
/// Translates <see cref="Result"/> values into HTTP responses so no controller
/// decides status codes for itself.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ApiControllerBase(ISender sender) => Sender = sender;

    /// <summary>Dispatches a command or query. Injected, not resolved from the container.</summary>
    protected ISender Sender { get; }

    /// <summary>The caller fingerprint, mapped from the connection rather than the body.</summary>
    protected ClientInfo Client => ClientInfo.Create(
        HttpContext.Connection.RemoteIpAddress?.ToString(),
        HttpContext.Request.Headers.UserAgent.ToString());

    /// <summary>
    /// True when the browser went away mid-request (navigation, tab close):
    /// the request-aborted token is signaled. Use it as a catch filter for
    /// <see cref="OperationCanceledException"/> so a disconnect answers 499
    /// inside user code instead of escaping to the framework — escaping pops
    /// the Visual Studio debugger even though the pipeline would handle it
    /// quietly. Any cancellation that is NOT a disconnect still propagates.
    /// </summary>
    protected bool ClientWentAway()
        => HttpContext.RequestAborted.IsCancellationRequested;

    protected ActionResult ToProblem(Error error)
    {
        var statusCode = error.Type.ToStatusCode();

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToTitle(),
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{Request.Method} {Request.Path}"
        };

        problem.Extensions["code"] = error.Code;
        problem.Extensions["requestId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <summary>
    /// Maps a successful result to 200 with its payload, or a failure to the
    /// matching ProblemDetails. Returning <see cref="ActionResult{T}"/> rather than
    /// a bare <c>ActionResult</c> is what lets each action declare the exact type
    /// it responds with, visible in its own signature.
    /// </summary>
    protected ActionResult<T> ToResponse<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            return ToProblem(result.Error!);
        }

        return Ok(result.Value);
    }

    /// <summary>For actions with no payload: 204 on success, ProblemDetails otherwise.</summary>
    protected ActionResult ToNoContent(Result result)
        => result.IsSuccess ? NoContent() : ToProblem(result.Error!);
}
