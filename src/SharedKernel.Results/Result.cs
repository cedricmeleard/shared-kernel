using System.Diagnostics.CodeAnalysis;

namespace SharedKernel.Results;

public union Result(Success, Error) : IResult<Result>, IEquatable<Result>
{
    public bool IsSuccess => this is Results.Success;

    public bool IsFailure => this is Results.Error;

    public Error Error => this is Results.Error error ? error : Error.None;

    public static Result Success() => Results.Success.Instance;

    public static Result Failure(Error error) =>
        error == Error.None
            ? throw new InvalidOperationException("Failure result must have an error.")
            : error;

    public static Result<TValue> Success<TValue>(TValue value) => new Success<TValue>(value);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public bool Equals(Result other) => Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public static bool operator ==(Result left, Result right) => left.Equals(right);

    public static bool operator !=(Result left, Result right) => !left.Equals(right);
}

public union Result<TValue>(Success<TValue>, Error) : IResult<Result<TValue>>, IEquatable<Result<TValue>>
{
    public bool IsSuccess => this is Success<TValue>;

    public bool IsFailure => this is Results.Error;

    public Error Error => this is Results.Error error ? error : Error.None;

    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        if (this is Success<TValue>(var innerValue))
        {
            value = innerValue;
            return true;
        }

        value = default;
        return false;
    }

    public TValue GetValueOrThrow() =>
        this switch
        {
            Success<TValue>(var innerValue) => innerValue,
            Results.Error => throw new InvalidOperationException("The value of a failure result can not be accessed."),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Result<TValue> Failure(Error error) =>
        error == Error.None
            ? throw new InvalidOperationException("Failure result must have an error.")
            : error;

    public static implicit operator Result<TValue>(TValue? value) => Result.Create(value);

    public bool Equals(Result<TValue> other) => Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(other);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);
}
