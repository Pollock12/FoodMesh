namespace FoodMesh.Shared.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Helps avoid throwing exceptions for routine control flow.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("A successful result cannot contain an error message.");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("A failed result must contain an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

/// <summary>
/// Represents the typed result of an operation containing a value on success.
/// </summary>
/// <typeparam name="T">The type of the data returned.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException($"Cannot access Value of a failure result: {Error}");

            return _value!;
        }
    }

    public static Result<T> Success(T value) => new(value, true, null);
    public static new Result<T> Failure(string error) => new(default, false, error);
}
