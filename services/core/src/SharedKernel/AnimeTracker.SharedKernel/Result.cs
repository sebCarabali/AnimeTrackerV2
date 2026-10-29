namespace AnimeTracker.SharedKernel;

public sealed class Result<T>
{
    private Result(T? value, string? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string error) => new(default, error, false);
}

public sealed class Result
{
    private Result(string? error, bool isSuccess)
    {
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(null, true);
    public static Result Failure(string error) => new(error, false);
}
