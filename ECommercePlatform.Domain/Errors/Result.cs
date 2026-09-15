namespace ECommercePlatform.Domain.Errors;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Ok(value);

    public static Result<T> Failure<T>(Error error) => Result<T>.Fail(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Only valid when <see cref="Result.IsSuccess"/> is true.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    internal static Result<T> Ok(T value) => new(true, value, null);

    internal static Result<T> Fail(Error error) => new(false, default, error);
}
