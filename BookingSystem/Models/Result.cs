namespace BookingSystem.Models;

public sealed record Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess && Value is not null ? onSuccess(Value) : onFailure(ErrorMessage);
}