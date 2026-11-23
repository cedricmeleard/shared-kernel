namespace SharedKernel.Results;

public static class ResultExtensions
{
    #region Bind

    public static Result Bind(this Result result, Func<Result> next) =>
        result switch
        {
            Success => next(),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Result<TOut> Bind<TOut>(this Result result, Func<Result<TOut>> next) =>
        result switch
        {
            Success => next(),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result> Bind(this Result result, Func<Task<Result>> next) =>
        result switch
        {
            Success => next(),
            Error error => Task.FromResult<Result>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result<TOut>> Bind<TOut>(this Result result, Func<Task<Result<TOut>>> next) =>
        result switch
        {
            Success => next(),
            Error error => Task.FromResult<Result<TOut>>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Result> next) =>
        (await resultTask).Bind(next);

    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Task<Result>> next) =>
        await (await resultTask).Bind(next);

    public static async Task<Result<TOut>> Bind<TOut>(this Task<Result> resultTask, Func<Result<TOut>> next) =>
        (await resultTask).Bind(next);

    public static async Task<Result<TOut>> Bind<TOut>(this Task<Result> resultTask, Func<Task<Result<TOut>>> next) =>
        await (await resultTask).Bind(next);

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Result Bind<TIn>(this Result<TIn> result, Func<TIn, Result> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result<TOut>> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => Task.FromResult<Result<TOut>>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result> Bind<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => Task.FromResult<Result>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> next) =>
        (await resultTask).Bind(next);

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next) =>
        await (await resultTask).Bind(next);

    public static async Task<Result> Bind<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result> next) =>
        (await resultTask).Bind(next);

    public static async Task<Result> Bind<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result>> next) =>
        await (await resultTask).Bind(next);

    #endregion

    #region Map

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper) =>
        result switch
        {
            Success<TIn>(var value) => Result.Success(mapper(value)),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper) =>
        result switch
        {
            Success<TIn>(var value) => Result.Success(await mapper(value)),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper) =>
        (await resultTask).Map(mapper);

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper) =>
        await (await resultTask).Map(mapper);

    #endregion

    #region Tap

    public static Result<TIn> Tap<TIn>(this Result<TIn> result, Action<TIn> action)
    {
        if (result is Success<TIn>(var value))
            action(value);
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Result<TIn> result, Func<TIn, Task> action)
    {
        if (result is Success<TIn>(var value))
            await action(value);
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Task<Result<TIn>> resultTask, Action<TIn> action) =>
        (await resultTask).Tap(action);

    public static async Task<Result<TIn>> Tap<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task> action) =>
        await (await resultTask).Tap(action);

    #endregion

    #region Ensure

    public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, bool> predicate, Error error) =>
        result switch
        {
            Success<TIn>(var value) => predicate(value) ? result : error,
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<bool>> predicate,
        Error error) =>
        result switch
        {
            Success<TIn>(var value) => await predicate(value) ? result : error,
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, bool> predicate,
        Error error) =>
        (await resultTask).Ensure(predicate, error);

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<bool>> predicate,
        Error error) =>
        await (await resultTask).Ensure(predicate, error);

    #endregion
}
