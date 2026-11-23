using FluentAssertions;
using SharedKernel.Results;
using Xunit;

namespace SharedKernel.UnitTests.Results;

public class ResultBindTests
{
    private static readonly Error InitialError = new("Bind.Initial", "Initial error");
    private static readonly Error NextError = new("Bind.Next", "Next error");

    #region Non-generic Result -> Result

    [Fact]
    public void Bind_ResultToResult_Should_ExecuteNext_WhenSuccess()
    {
        var result = Result.Success().Bind(() => Result.Failure(NextError));

        result.Value.Should().Be(NextError);
    }

    [Fact]
    public void Bind_ResultToResult_Should_ShortCircuit_WhenFailure()
    {
        var called = false;

        var result = Result.Failure(InitialError).Bind(() =>
        {
            called = true;
            return Result.Success();
        });

        called.Should().BeFalse();
        result.Value.Should().Be(InitialError);
    }

    [Fact]
    public void Bind_ResultToResult_Should_Throw_WhenUninitialized()
    {
        Result uninitialized = default;

        var act = () => uninitialized.Bind(Result.Success);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Bind_ResultToTaskResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Result.Success().Bind(() => Task.FromResult(Result.Success()));
        var err = await Result.Failure(InitialError).Bind(() =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success());
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultToResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success()).Bind(Result.Success);
        var err = await Task.FromResult(Result.Failure(InitialError)).Bind(() =>
        {
            calledOnFailure = true;
            return Result.Success();
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultToTaskResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success()).Bind(() => Task.FromResult(Result.Success()));
        var err = await Task.FromResult(Result.Failure(InitialError)).Bind(() =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success());
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    #endregion

    #region Non-generic Result -> Result<TOut>

    [Fact]
    public void Bind_ResultToResultT_Should_ExecuteNext_WhenSuccess()
    {
        var result = Result.Success().Bind(() => Result.Success(42));

        result.Value.Should().Be(new Success<int>(42));
    }

    [Fact]
    public void Bind_ResultToResultT_Should_ShortCircuit_WhenFailure()
    {
        var called = false;

        var result = Result.Failure(InitialError).Bind(() =>
        {
            called = true;
            return Result.Success(42);
        });

        called.Should().BeFalse();
        result.Value.Should().Be(InitialError);
    }

    [Fact]
    public void Bind_ResultToResultT_Should_Throw_WhenUninitialized()
    {
        Result uninitialized = default;

        var act = () => uninitialized.Bind(() => Result.Success(42));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Bind_ResultToTaskResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Result.Success().Bind(() => Task.FromResult(Result.Success("ok")));
        var err = await Result.Failure(InitialError).Bind(() =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success("ok"));
        });

        ok.Value.Should().Be(new Success<string>("ok"));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultToResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success()).Bind(() => Result.Success(99));
        var err = await Task.FromResult(Result.Failure(InitialError)).Bind(() =>
        {
            calledOnFailure = true;
            return Result.Success(99);
        });

        ok.Value.Should().Be(new Success<int>(99));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultToTaskResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success()).Bind(() => Task.FromResult(Result.Success(99)));
        var err = await Task.FromResult(Result.Failure(InitialError)).Bind(() =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success(99));
        });

        ok.Value.Should().Be(new Success<int>(99));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    #endregion

    #region Generic Result<TIn> -> Result<TOut>

    [Fact]
    public void Bind_ResultTToResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = Result.Success(5).Bind(v => Result.Success(v * 2));
        var nextErr = Result.Success(5).Bind(_ => Result.Failure<int>(NextError));
        var shortCircuited = Result.Failure<int>(InitialError).Bind(v =>
        {
            calledOnFailure = true;
            return Result.Success(v * 2);
        });

        ok.Value.Should().Be(new Success<int>(10));
        nextErr.Value.Should().Be(NextError);
        calledOnFailure.Should().BeFalse();
        shortCircuited.Value.Should().Be(InitialError);
    }

    [Fact]
    public void Bind_ResultTToResultT_Should_Throw_WhenUninitialized()
    {
        Result<int> uninitialized = default;

        var act = () => uninitialized.Bind(v => Result.Success(v * 2));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Bind_ResultTToTaskResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Result.Success(5).Bind(v => Task.FromResult(Result.Success(v * 3)));
        var err = await Result.Failure<int>(InitialError).Bind(v =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success(v * 3));
        });

        ok.Value.Should().Be(new Success<int>(15));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultTToResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success(5)).Bind(v => Result.Success(v + 1));
        var err = await Task.FromResult(Result.Failure<int>(InitialError)).Bind(v =>
        {
            calledOnFailure = true;
            return Result.Success(v + 1);
        });

        ok.Value.Should().Be(new Success<int>(6));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultTToTaskResultT_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success(5)).Bind(v => Task.FromResult(Result.Success(v + 2)));
        var err = await Task.FromResult(Result.Failure<int>(InitialError)).Bind(v =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success(v + 2));
        });

        ok.Value.Should().Be(new Success<int>(7));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    #endregion

    #region Generic Result<TIn> -> Result

    [Fact]
    public void Bind_ResultTToResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = Result.Success(5).Bind(v => v > 0 ? Result.Success() : NextError);
        var nextErr = Result.Success(-1).Bind(v => v > 0 ? Result.Success() : NextError);
        var shortCircuited = Result.Failure<int>(InitialError).Bind(_ =>
        {
            calledOnFailure = true;
            return Result.Success();
        });

        ok.Value.Should().Be(Success.Instance);
        nextErr.Value.Should().Be(NextError);
        calledOnFailure.Should().BeFalse();
        shortCircuited.Value.Should().Be(InitialError);
    }

    [Fact]
    public void Bind_ResultTToResult_Should_Throw_WhenUninitialized()
    {
        Result<int> uninitialized = default;

        var act = () => uninitialized.Bind(_ => Result.Success());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Bind_ResultTToTaskResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Result.Success(5).Bind(_ => Task.FromResult(Result.Success()));
        var err = await Result.Failure<int>(InitialError).Bind(_ =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success());
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultTToResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success(5)).Bind(_ => Result.Success());
        var err = await Task.FromResult(Result.Failure<int>(InitialError)).Bind(_ =>
        {
            calledOnFailure = true;
            return Result.Success();
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Bind_TaskResultTToTaskResult_Should_ExecuteOrShortCircuit()
    {
        var calledOnFailure = false;

        var ok = await Task.FromResult(Result.Success(5)).Bind(_ => Task.FromResult(Result.Success()));
        var err = await Task.FromResult(Result.Failure<int>(InitialError)).Bind(_ =>
        {
            calledOnFailure = true;
            return Task.FromResult(Result.Success());
        });

        ok.Value.Should().Be(Success.Instance);
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    #endregion

    [Fact]
    public async Task UninitializedAsyncOverloads_Should_ThrowInvalidOperationException()
    {
        Result uninit = default;
        Result<int> uninitT = default;

        await Assert.ThrowsAsync<InvalidOperationException>(() => uninit.Bind(() => Task.FromResult(Result.Success())));
        await Assert.ThrowsAsync<InvalidOperationException>(() => uninit.Bind(() => Task.FromResult(Result.Success(1))));
        await Assert.ThrowsAsync<InvalidOperationException>(() => uninitT.Bind(x => Task.FromResult(Result.Success(x))));
        await Assert.ThrowsAsync<InvalidOperationException>(() => uninitT.Bind(_ => Task.FromResult(Result.Success())));
    }
}
