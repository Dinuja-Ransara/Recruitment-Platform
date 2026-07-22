namespace Meridian.Application.Common.Models;

/// <summary>
/// Explicit success/failure carrier. Used so that expected outcomes such as
/// "email already registered" travel as values rather than as exceptions.
/// </summary>
public class Result<T>
{
    public bool Succeeded { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool succeeded, T? value, string? error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
