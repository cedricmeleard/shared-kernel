using FluentAssertions;
using SharedKernel.Results;
using Xunit;

namespace SharedKernel.UnitTests.Results;

public class ResultRopCombinatorsTests
{
    private static readonly Error InitialError = new("Rop.Initial", "Initial error");
    private static readonly Error ValidationError = new("Rop.Validation", "Validation failed");

    #region Map

    [Fact]
    public void Map_Sync_Should_TransformSuccess_AndShortCircuitFailure()
    {
        var calledOnFailure = false;

        var ok = Result.Success("hello").Map(s => s.ToUpperInvariant());
        var err = Result.Failure<string>(InitialError).Map(s =>
        {
            calledOnFailure = true;
            return s.ToUpperInvariant();
        });

        ok.Value.Should().Be(new Success<string>("HELLO"));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Map_AsyncOverloads_Should_TransformSuccess_AndShortCircuitFailure()
    {
        var calledCount = 0;

        var okSyncToAsync = await Result.Success(3).Map(async v =>
        {
            await Task.Yield();
            return v * 2;
        });
        var errSyncToAsync = await Result.Failure<int>(InitialError).Map(async v =>
        {
            calledCount++;
            await Task.Yield();
            return v * 2;
        });

        var okTaskToSync = await Task.FromResult(Result.Success(4)).Map(v => v * 2);
        var errTaskToSync = await Task.FromResult(Result.Failure<int>(InitialError)).Map(v =>
        {
            calledCount++;
            return v * 2;
        });

        var okTaskToAsync = await Task.FromResult(Result.Success(5)).Map(async v =>
        {
            await Task.Yield();
            return v * 2;
        });
        var errTaskToAsync = await Task.FromResult(Result.Failure<int>(InitialError)).Map(async v =>
        {
            calledCount++;
            await Task.Yield();
            return v * 2;
        });

        okSyncToAsync.Value.Should().Be(new Success<int>(6));
        errSyncToAsync.Value.Should().Be(InitialError);
        okTaskToSync.Value.Should().Be(new Success<int>(8));
        errTaskToSync.Value.Should().Be(InitialError);
        okTaskToAsync.Value.Should().Be(new Success<int>(10));
        errTaskToAsync.Value.Should().Be(InitialError);
        calledCount.Should().Be(0);
    }

    [Fact]
    public async Task Map_Should_Throw_WhenUninitialized()
    {
        Result<int> uninit = default;

        var syncAct = () => uninit.Map(x => x + 1);
        syncAct.Should().Throw<InvalidOperationException>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => uninit.Map(x => Task.FromResult(x + 1)));
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_Sync_Should_ExecuteOnlyOnSuccess_AndReturnOriginalResult()
    {
        var captured = 0;
        var calledOnFailure = false;

        var ok = Result.Success(42).Tap(v => captured = v);
        var err = Result.Failure<int>(InitialError).Tap(_ => calledOnFailure = true);

        captured.Should().Be(42);
        ok.Value.Should().Be(new Success<int>(42));
        calledOnFailure.Should().BeFalse();
        err.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Tap_AsyncOverloads_Should_ExecuteOnlyOnSuccess()
    {
        var log = new List<int>();

        var r1 = await Result.Success(1).Tap(async v =>
        {
            await Task.Yield();
            log.Add(v);
        });
        var e1 = await Result.Failure<int>(InitialError).Tap(async v =>
        {
            await Task.Yield();
            log.Add(v);
        });

        var r2 = await Task.FromResult(Result.Success(2)).Tap(v => log.Add(v));
        var e2 = await Task.FromResult(Result.Failure<int>(InitialError)).Tap(v => log.Add(v));

        var r3 = await Task.FromResult(Result.Success(3)).Tap(async v =>
        {
            await Task.Yield();
            log.Add(v);
        });
        var e3 = await Task.FromResult(Result.Failure<int>(InitialError)).Tap(async v =>
        {
            await Task.Yield();
            log.Add(v);
        });

        log.Should().Equal(1, 2, 3);
        r1.Value.Should().Be(new Success<int>(1));
        e1.Value.Should().Be(InitialError);
        r2.Value.Should().Be(new Success<int>(2));
        e2.Value.Should().Be(InitialError);
        r3.Value.Should().Be(new Success<int>(3));
        e3.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Tap_Should_Throw_WhenUninitialized()
    {
        Result<int> uninit = default;

        var syncAct = () => uninit.Tap(_ => { });
        syncAct.Should().Throw<InvalidOperationException>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => uninit.Tap(_ => Task.CompletedTask));
    }

    #endregion

    #region Ensure

    [Fact]
    public void Ensure_Sync_Should_EvaluatePredicateOnSuccess_AndPreserveInitialErrorOnFailure()
    {
        var calledOnFailure = false;

        var passed = Result.Success(25).Ensure(v => v > 20, ValidationError);
        var failed = Result.Success(10).Ensure(v => v > 20, ValidationError);
        var shortCircuited = Result.Failure<int>(InitialError).Ensure(v =>
        {
            calledOnFailure = true;
            return v > 20;
        }, ValidationError);

        passed.Value.Should().Be(new Success<int>(25));
        failed.Value.Should().Be(ValidationError);
        calledOnFailure.Should().BeFalse();
        shortCircuited.Value.Should().Be(InitialError);
    }

    [Fact]
    public async Task Ensure_AsyncOverloads_Should_EvaluatePredicateOnSuccess_AndShortCircuitOnFailure()
    {
        var calledOnFailure = 0;

        var ok1 = await Result.Success(10).Ensure(async v =>
        {
            await Task.Yield();
            return v > 5;
        }, ValidationError);
        var fail1 = await Result.Success(2).Ensure(async v =>
        {
            await Task.Yield();
            return v > 5;
        }, ValidationError);
        var err1 = await Result.Failure<int>(InitialError).Ensure(async v =>
        {
            calledOnFailure++;
            await Task.Yield();
            return v > 5;
        }, ValidationError);

        var ok2 = await Task.FromResult(Result.Success(10)).Ensure(v => v > 5, ValidationError);
        var fail2 = await Task.FromResult(Result.Success(2)).Ensure(v => v > 5, ValidationError);
        var err2 = await Task.FromResult(Result.Failure<int>(InitialError)).Ensure(v =>
        {
            calledOnFailure++;
            return v > 5;
        }, ValidationError);

        var ok3 = await Task.FromResult(Result.Success(10)).Ensure(async v =>
        {
            await Task.Yield();
            return v > 5;
        }, ValidationError);
        var fail3 = await Task.FromResult(Result.Success(2)).Ensure(async v =>
        {
            await Task.Yield();
            return v > 5;
        }, ValidationError);
        var err3 = await Task.FromResult(Result.Failure<int>(InitialError)).Ensure(async v =>
        {
            calledOnFailure++;
            await Task.Yield();
            return v > 5;
        }, ValidationError);

        ok1.Value.Should().Be(new Success<int>(10));
        fail1.Value.Should().Be(ValidationError);
        err1.Value.Should().Be(InitialError);

        ok2.Value.Should().Be(new Success<int>(10));
        fail2.Value.Should().Be(ValidationError);
        err2.Value.Should().Be(InitialError);

        ok3.Value.Should().Be(new Success<int>(10));
        fail3.Value.Should().Be(ValidationError);
        err3.Value.Should().Be(InitialError);

        calledOnFailure.Should().Be(0);
    }

    [Fact]
    public async Task Ensure_Should_Throw_WhenUninitialized()
    {
        Result<int> uninit = default;

        var syncAct = () => uninit.Ensure(_ => true, ValidationError);
        syncAct.Should().Throw<InvalidOperationException>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => uninit.Ensure(_ => Task.FromResult(true), ValidationError));
    }

    #endregion

    #region End-to-end ROP Pipelines

    [Fact]
    public async Task Pipeline_Should_ExecuteAllSteps_WhenEverythingSucceeds()
    {
        var steps = new List<string>();

        var output = await Task.FromResult(Result.Success())
            .Bind(() =>
            {
                steps.Add("bind-1");
                return Task.FromResult(Result.Success(10));
            })
            .Tap(async v =>
            {
                await Task.Yield();
                steps.Add($"tap-1:{v}");
            })
            .Ensure(async v =>
            {
                await Task.Yield();
                steps.Add("ensure");
                return v > 5;
            }, ValidationError)
            .Map(async v =>
            {
                await Task.Yield();
                steps.Add("map");
                return v * 3;
            })
            .Bind(v =>
            {
                steps.Add("bind-2");
                return Result.Success($"total={v}");
            }) switch
        {
            Success<string>(var value) => value,
            Error err => err.Code
        };

        steps.Should().Equal("bind-1", "tap-1:10", "ensure", "map", "bind-2");
        output.Should().Be("total=30");
    }

    [Fact]
    public async Task Pipeline_Should_ShortCircuitAllRemainingSteps_WhenEnsureFails()
    {
        var steps = new List<string>();

        var output = await Task.FromResult(Result.Success())
            .Bind(() =>
            {
                steps.Add("bind-1");
                return Result.Success(2);
            })
            .Ensure(v =>
            {
                steps.Add("ensure");
                return v > 5;
            }, ValidationError)
            .Tap(v => steps.Add($"tap:{v}"))
            .Map(async v =>
            {
                await Task.Yield();
                steps.Add("map");
                return v * 10;
            })
            .Bind(v =>
            {
                steps.Add("bind-2");
                return Task.FromResult(Result.Success(v));
            }) switch
        {
            Success<int>(var value) => $"ok:{value}",
            Error err => $"err:{err.Code}"
        };

        steps.Should().Equal("bind-1", "ensure");
        output.Should().Be($"err:{ValidationError.Code}");
    }

    #endregion
}
