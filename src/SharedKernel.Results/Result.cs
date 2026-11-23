namespace SharedKernel.Results;

public union Result(Success, Error) : IResult<Result>
{
    public static Result Success() => Results.Success.Instance;

    public static Result Failure(Error error) =>
        error == Error.None
            ? throw new InvalidOperationException("Failure result must have an error.")
            : error;

    public static Result<TValue> Success<TValue>(TValue value) => new Success<TValue>(value);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

public union Result<TValue>(Success<TValue>, Error) : IResult<Result<TValue>>
{
    public static Result<TValue> Failure(Error error) =>
        error == Error.None
            ? throw new InvalidOperationException("Failure result must have an error.")
            : error;

    public static implicit operator Result<TValue>(TValue? value) => Result.Create(value);
}
