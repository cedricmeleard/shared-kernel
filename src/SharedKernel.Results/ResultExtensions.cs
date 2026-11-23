namespace SharedKernel.Results;

public static class ResultExtensions
{
    #region Match Overloads

    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result switch
        {
            Success => onSuccess(),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static void Match(
        this Result result,
        Action onSuccess,
        Action<Error> onFailure)
    {
        switch (result)
        {
            case Success:
                onSuccess();
                break;
            case Error error:
                onFailure(error);
                break;
            default:
                throw new InvalidOperationException("Result is in an uninitialized state.");
        }
    }

    public static Task<TOut> Match<TOut>(
        this Result result,
        Func<Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure) =>
        result switch
        {
            Success => onSuccess(),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<TOut> Match<TOut>(
        this Task<Result> resultTask,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TOut> Match<TOut>(
        this Task<Result> resultTask,
        Func<Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return await result.Match(onSuccess, onFailure);
    }

    public static Task Match(
        this Result result,
        Func<Task> onSuccess,
        Func<Error, Task> onFailure) =>
        result switch
        {
            Success => onSuccess(),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task Match(
        this Task<Result> resultTask,
        Action onSuccess,
        Action<Error> onFailure)
    {
        var result = await resultTask;
        result.Match(onSuccess, onFailure);
    }

    public static async Task Match(
        this Task<Result> resultTask,
        Func<Task> onSuccess,
        Func<Error, Task> onFailure)
    {
        var result = await resultTask;
        await result.Match(onSuccess, onFailure);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure) =>
        result switch
        {
            Success<TIn>(var value) => onSuccess(value),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static void Match<TIn>(
        this Result<TIn> result,
        Action<TIn> onSuccess,
        Action<Error> onFailure)
    {
        switch (result)
        {
            case Success<TIn>(var value):
                onSuccess(value);
                break;
            case Error error:
                onFailure(error);
                break;
            default:
                throw new InvalidOperationException("Result is in an uninitialized state.");
        }
    }

    public static Task<TOut> Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure) =>
        result switch
        {
            Success<TIn>(var value) => onSuccess(value),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task Match<TIn>(
        this Result<TIn> result,
        Func<TIn, Task> onSuccess,
        Func<Error, Task> onFailure) =>
        result switch
        {
            Success<TIn>(var value) => onSuccess(value),
            Error error => onFailure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<TOut> Match<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<TOut> Match<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        var result = await resultTask;
        return await result.Match(onSuccess, onFailure);
    }

    public static async Task Match<TIn>(
        this Task<Result<TIn>> resultTask,
        Action<TIn> onSuccess,
        Action<Error> onFailure)
    {
        var result = await resultTask;
        result.Match(onSuccess, onFailure);
    }

    public static async Task Match<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task> onSuccess,
        Func<Error, Task> onFailure)
    {
        var result = await resultTask;
        await result.Match(onSuccess, onFailure);
    }

    #endregion

    #region Bind Overloads

    public static Result Bind(
        this Result result,
        Func<Result> next) =>
        result switch
        {
            Success => next(),
            Error error => error,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Result<TOut> Bind<TOut>(
        this Result result,
        Func<Result<TOut>> next) =>
        result switch
        {
            Success => next(),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result> Bind(
        this Result result,
        Func<Task<Result>> next) =>
        result.Match(next, error => Task.FromResult(Result.Failure(error)));

    public static Task<Result<TOut>> Bind<TOut>(
        this Result result,
        Func<Task<Result<TOut>>> next) =>
        result.Match(next, error => Task.FromResult(Result.Failure<TOut>(error)));

    public static async Task<Result> Bind(
        this Task<Result> resultTask,
        Func<Result> next)
    {
        var result = await resultTask;
        return result.Bind(next);
    }

    public static async Task<Result> Bind(
        this Task<Result> resultTask,
        Func<Task<Result>> next)
    {
        var result = await resultTask;
        return await result.Bind(next);
    }

    public static async Task<Result<TOut>> Bind<TOut>(
        this Task<Result> resultTask,
        Func<Result<TOut>> next)
    {
        var result = await resultTask;
        return result.Bind(next);
    }

    public static async Task<Result<TOut>> Bind<TOut>(
        this Task<Result> resultTask,
        Func<Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return await result.Bind(next);
    }

    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Result Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Result> next) =>
        result switch
        {
            Success<TIn>(var value) => next(value),
            Error error => Result.Failure(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static Task<Result<TOut>> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next) =>
        result.Match(next, error => Task.FromResult(Result.Failure<TOut>(error)));

    public static Task<Result> Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<Result>> next) =>
        result.Match(next, error => Task.FromResult(Result.Failure(error)));

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return await result.Bind(next);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> next)
    {
        var result = await resultTask;
        return result.Bind(next);
    }

    public static async Task<Result> Bind<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result>> next)
    {
        var result = await resultTask;
        return await result.Bind(next);
    }

    public static async Task<Result> Bind<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result> next)
    {
        var result = await resultTask;
        return result.Bind(next);
    }

    #endregion

    #region Map Overloads

    public static Result<TOut> Map<TOut>(
        this Result result,
        Func<TOut> mapper) =>
        result switch
        {
            Success => Result.Success(mapper()),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TOut>(
        this Result result,
        Func<Task<TOut>> mapper) =>
        result switch
        {
            Success => Result.Success(await mapper()),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TOut>(
        this Task<Result> resultTask,
        Func<TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<TOut>> Map<TOut>(
        this Task<Result> resultTask,
        Func<Task<TOut>> mapper)
    {
        var result = await resultTask;
        return await result.Map(mapper);
    }

    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper) =>
        result switch
        {
            Success<TIn>(var value) => Result.Success(mapper(value)),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper) =>
        result switch
        {
            Success<TIn>(var value) => Result.Success(await mapper(value)),
            Error error => Result.Failure<TOut>(error),
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return await result.Map(mapper);
    }

    #endregion

    #region Tap Overloads

    public static Result Tap(
        this Result result,
        Action action)
    {
        if (result is Success)
            action();
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result> Tap(
        this Result result,
        Func<Task> action)
    {
        if (result is Success)
            await action();
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result> Tap(
        this Task<Result> resultTask,
        Action action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    public static async Task<Result> Tap(
        this Task<Result> resultTask,
        Func<Task> action)
    {
        var result = await resultTask;
        return await result.Tap(action);
    }

    public static Result<TIn> Tap<TIn>(
        this Result<TIn> result,
        Action<TIn> action)
    {
        if (result is Success<TIn>(var value))
            action(value);
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(
        this Result<TIn> result,
        Func<TIn, Task> action)
    {
        if (result is Success<TIn>(var value))
            await action(value);
        else if (result is not Error)
            throw new InvalidOperationException("Result is in an uninitialized state.");

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(
        this Task<Result<TIn>> resultTask,
        Action<TIn> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    public static async Task<Result<TIn>> Tap<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task> action)
    {
        var result = await resultTask;
        return await result.Tap(action);
    }

    #endregion

    #region Ensure Overloads

    public static Result Ensure(
        this Result result,
        Func<bool> predicate,
        Error error) =>
        result switch
        {
            Success => predicate() ? result : Result.Failure(error),
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result> Ensure(
        this Result result,
        Func<Task<bool>> predicate,
        Error error) =>
        result switch
        {
            Success => await predicate() ? result : Result.Failure(error),
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result> Ensure(
        this Task<Result> resultTask,
        Func<bool> predicate,
        Error error)
    {
        var result = await resultTask;
        return result.Ensure(predicate, error);
    }

    public static async Task<Result> Ensure(
        this Task<Result> resultTask,
        Func<Task<bool>> predicate,
        Error error)
    {
        var result = await resultTask;
        return await result.Ensure(predicate, error);
    }

    public static Result<TIn> Ensure<TIn>(
        this Result<TIn> result,
        Func<TIn, bool> predicate,
        Error error) =>
        result switch
        {
            Success<TIn>(var value) => predicate(value) ? result : Result.Failure<TIn>(error),
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<bool>> predicate,
        Error error) =>
        result switch
        {
            Success<TIn>(var value) => await predicate(value) ? result : Result.Failure<TIn>(error),
            Error => result,
            _ => throw new InvalidOperationException("Result is in an uninitialized state.")
        };

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, bool> predicate,
        Error error)
    {
        var result = await resultTask;
        return result.Ensure(predicate, error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<bool>> predicate,
        Error error)
    {
        var result = await resultTask;
        return await result.Ensure(predicate, error);
    }

    #endregion
}
