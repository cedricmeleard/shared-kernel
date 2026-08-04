using System.Diagnostics.CodeAnalysis;

namespace SharedKernel.Results;

public class Result
{
    protected Result(bool isSuccess,
        Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess
    {
        get;
    }

    public bool IsFailure => !IsSuccess;

    public Error Error
    {
        get;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    #region Railway-Oriented Programming (ROP) Combinators

    public TOut Match<TOut>(Func<TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    public void Match(Action onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error);
    }

    public Result Bind(Func<Result> next) => IsSuccess ? next() : Failure(Error);

    public Result<TOut> Bind<TOut>(Func<Result<TOut>> next) => IsSuccess ? next() : Failure<TOut>(Error);

    public Result<TOut> Map<TOut>(Func<TOut> mapper) =>
        IsSuccess ? Success(mapper()) : Failure<TOut>(Error);

    public Result Tap(Action action)
    {
        if (IsSuccess) action();

        return this;
    }

    public Result Ensure(Func<bool> predicate,
        Error error)
    {
        if (IsFailure) return this;

        return predicate() ? this : Failure(error);
    }

    #endregion
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error) =>
        _value = value;

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);

    #region Railway-Oriented Programming (ROP) Combinators

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    public void Match(Action<TValue> onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Error);
    }

    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> next) =>
        IsSuccess ? next(Value) : Failure<TOut>(Error);

    public Result Bind(Func<TValue, Result> next) => IsSuccess ? next(Value) : Failure(Error);

    public Result<TOut> Map<TOut>(Func<TValue, TOut> mapper) =>
        IsSuccess ? Success(mapper(Value)) : Failure<TOut>(Error);

    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess) action(Value);

        return this;
    }

    public Result<TValue> Ensure(Predicate<TValue> predicate,
        Error error)
    {
        if (IsFailure) return this;

        return predicate(Value) ? this : Failure<TValue>(error);
    }

    #endregion
}