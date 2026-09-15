using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Common.Messaging.Behaviors;

/// <summary>
/// Validates the request's data annotations before the handler runs, so handlers
/// can assume well-formed input.
/// </summary>
/// <remarks>
/// For HTTP callers this mostly duplicates what <c>[ApiController]</c> already
/// does at model binding. It earns its place for everything that does not arrive
/// over HTTP — the seeder, background jobs, tests, and any future queue consumer —
/// which would otherwise reach handlers unchecked.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Builds the "turn an Error into a failed TResponse" function once per closed
    /// generic pair; the reflection cost is paid at first use, not per request.
    /// </summary>
    private static readonly Func<Error, TResponse>? FailureFactory = BuildFailureFactory();

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            return next(cancellationToken);
        }

        var message = string.Join(" ", results
            .Select(r => r.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m)));

        var error = Error.Validation("request.validation_failed", message);

        if (FailureFactory is null)
        {
            // Only reachable if a handler returns something that is not a Result,
            // which leaves nowhere to put the failure. A programming error.
            throw new InvalidOperationException(
                $"'{typeof(TRequest).Name}' failed validation, but its response type "
                + $"'{typeof(TResponse).Name}' is not a Result, so the failure cannot be returned. "
                + $"Validation errors: {message}");
        }

        return Task.FromResult(FailureFactory(error));
    }

    private static Func<Error, TResponse>? BuildFailureFactory()
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return error => (TResponse)(object)Result.Failure(error);
        }

        if (!typeof(TResponse).IsGenericType
            || typeof(TResponse).GetGenericTypeDefinition() != typeof(Result<>))
        {
            return null;
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failure = typeof(Result)
            .GetMethod(nameof(Result.Failure), genericParameterCount: 1,
                BindingFlags.Public | BindingFlags.Static, binder: null, types: [typeof(Error)], modifiers: null)!
            .MakeGenericMethod(valueType);

        return error => (TResponse)failure.Invoke(null, [error])!;
    }
}
