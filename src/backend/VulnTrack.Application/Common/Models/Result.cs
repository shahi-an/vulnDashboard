namespace VulnTrack.Application.Common.Models;

public sealed class Result
{
    private Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }

    public static Result Success() => new(true, []);
    public static Result Failure(params string[] errors) => new(false, errors);
}

public sealed class Result<T>
{
    private Result(bool succeeded, T? value, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public string[] Errors { get; }

    public static Result<T> Success(T value) => new(true, value, []);
    public static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
