namespace Results;

public static class ResultExtensions
{
    #region Match Overloads

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
        return result.IsSuccess ? await onSuccess() : await onFailure(result.Error);
    }

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
        return result.IsSuccess ? await onSuccess(result.Value) : await onFailure(result.Error);
    }

    public static async Task<TOut> Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure) =>
        result.IsSuccess ? await onSuccess(result.Value) : await onFailure(result.Error);

    #endregion

    #region Bind Overloads

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next) =>
        result.IsSuccess ? await next(result.Value) : Result.Failure<TOut>(result.Error);

    public static async Task<Result> Bind<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<Result>> next) =>
        result.IsSuccess ? await next(result.Value) : Result.Failure(result.Error);

    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return result.IsSuccess ? await next(result.Value) : Result.Failure<TOut>(result.Error);
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
        return result.IsSuccess ? await next(result.Value) : Result.Failure(result.Error);
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

    public static async Task<Result<TOut>> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper) =>
        result.IsSuccess ? Result.Success(await mapper(result.Value)) : Result.Failure<TOut>(result.Error);

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
        return result.IsSuccess ? Result.Success(await mapper(result.Value)) : Result.Failure<TOut>(result.Error);
    }

    #endregion

    #region Tap Overloads

    public static async Task<Result<TIn>> Tap<TIn>(
        this Result<TIn> result,
        Func<TIn, Task> action)
    {
        if (result.IsSuccess) await action(result.Value);

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
        if (result.IsSuccess) await action(result.Value);

        return result;
    }

    #endregion

    #region Ensure Overloads

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Result<TIn> result,
        Func<TIn, Task<bool>> predicate,
        Error error)
    {
        if (result.IsFailure) return result;

        return await predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(
        this Task<Result<TIn>> resultTask,
        Predicate<TIn> predicate,
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
        if (result.IsFailure) return result;

        return await predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    #endregion
}