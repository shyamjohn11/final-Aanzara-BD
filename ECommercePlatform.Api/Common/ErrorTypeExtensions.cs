using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Api.Common;

/// <summary>
/// Maps domain error types onto HTTP. Lives in the Api layer because the mapping
/// is a transport concern — the Domain has no opinion about status codes.
/// </summary>
public static class ErrorTypeExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        _ => StatusCodes.Status500InternalServerError
    };

    public static string ToTitle(this ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation failed",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Conflict",
        ErrorType.TooManyRequests => "Too many requests",
        _ => "Unexpected error"
    };
}
