using FluentAssertions;
using SharedKernel.Results;
using Xunit;

namespace SharedKernel.UnitTests.Results;

public class ResultTests
{
    private static readonly Error TestError = new("Test.Error", "Test error message");

    [Fact]
    public void Success_Should_CreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_Should_CreateFailedResultWithError()
    {
        // Act
        var result = Result.Failure(TestError);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
    }

    [Fact]
    public void Failure_Should_Throw_WhenErrorIsNone()
    {
        // Act
        var act = () => Result.Failure(Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SuccessT_Should_ContainValue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.GetValueOrThrow().Should().Be(42);
        result.TryGetValue(out var value).Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void FailureT_Should_ThrowOnGetValueOrThrow_AndReturnFalseOnTryGetValue()
    {
        // Act
        var result = Result.Failure<int>(TestError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
        result.TryGetValue(out var value).Should().BeFalse();
        value.Should().Be(default);

        var act = () => result.GetValueOrThrow();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversions_Should_WorkForErrorAndValue()
    {
        // Act
        Result nonGenericError = TestError;
        Result<int> genericError = TestError;
        Result<string> genericValue = "hello";
        Result<string> nullValue = (string?)null;

        // Assert
        nonGenericError.IsFailure.Should().BeTrue();
        nonGenericError.Error.Should().Be(TestError);
        genericError.IsFailure.Should().BeTrue();
        genericError.Error.Should().Be(TestError);
        genericValue.IsSuccess.Should().BeTrue();
        genericValue.GetValueOrThrow().Should().Be("hello");
        nullValue.IsFailure.Should().BeTrue();
        nullValue.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void UnionSwitchExpression_Should_MatchExhaustively()
    {
        // Arrange
        Result nonGeneric = Result.Success();
        Result<int> success = Result.Success(42);
        Result<int> failure = TestError;

        // Act
        var nonGenericText = nonGeneric switch
        {
            Success => "ok",
            Error e => e.Code
        };

        var successText = success switch
        {
            Success<int>(var val) => $"ok:{val}",
            Error e => e.Code
        };

        var failureText = failure switch
        {
            Success<int>(var val) => $"ok:{val}",
            Error e => e.Code
        };

        // Assert
        nonGenericText.Should().Be("ok");
        successText.Should().Be("ok:42");
        failureText.Should().Be(TestError.Code);
    }

    [Fact]
    public void Equality_Should_CompareUnionCasesStructurally()
    {
        Result.Success().Should().Be(Result.Success());
        Result.Failure(TestError).Should().Be(Result.Failure(new Error("Test.Error", "Test error message")));
        Result.Success(42).Should().Be(Result.Success(42));
        Result.Success(42).Should().NotBe(Result.Success(99));
        (Result.Success(42) == Result.Success(42)).Should().BeTrue();
        (Result.Success(42) != Result.Failure<int>(TestError)).Should().BeTrue();
    }

    [Fact]
    public void Match_Should_ExecuteSuccessDelegate_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success(100);

        // Act
        var output = result.Match(
            value => $"Success: {value}",
            error => $"Failure: {error.Name}");

        // Assert
        output.Should().Be("Success: 100");
    }

    [Fact]
    public void Match_Should_ExecuteFailureDelegate_WhenResultIsFailure()
    {
        // Arrange
        var result = Result.Failure<int>(TestError);

        // Act
        var output = result.Match(
            value => $"Success: {value}",
            error => $"Failure: {error.Name}");

        // Assert
        output.Should().Be("Failure: Test error message");
    }

    [Fact]
    public async Task MatchAsync_Should_ExecuteSuccessDelegate_WhenResultIsSuccess()
    {
        // Arrange
        var resultTask = Task.FromResult(Result.Success("Hello"));

        // Act
        var output = await resultTask.Match(
            val => Task.FromResult($"Async: {val}"),
            err => Task.FromResult($"Error: {err.Name}"));

        // Assert
        output.Should().Be("Async: Hello");
    }

    [Fact]
    public void Bind_Should_ChainOperation_WhenSuccess()
    {
        // Arrange
        var initial = Result.Success(5);

        // Act
        var bound = initial.Bind(val => Result.Success(val * 2));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.GetValueOrThrow().Should().Be(10);
    }

    [Fact]
    public void Bind_Should_ReturnFailure_WhenInitialIsFailure()
    {
        // Arrange
        var initial = Result.Failure<int>(TestError);

        // Act
        var bound = initial.Bind(val => Result.Success(val * 2));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(TestError);
    }

    [Fact]
    public async Task NonGenericResult_AsyncRopPipeline_Should_ChainCleanly()
    {
        // Arrange
        var tapped = false;

        // Act
        var output = await Task.FromResult(Result.Success())
            .Tap(() =>
            {
                tapped = true;
                return Task.CompletedTask;
            })
            .Ensure(() => Task.FromResult(true), TestError)
            .Bind(() => Task.FromResult(Result.Success(21)))
            .Map(val => val * 2)
            .Match(
                val => $"Done:{val}",
                err => err.Code);

        // Assert
        tapped.Should().BeTrue();
        output.Should().Be("Done:42");
    }

    [Fact]
    public void Map_Should_TransformValue_WhenSuccess()
    {
        // Arrange
        var initial = Result.Success("hello");

        // Act
        var mapped = initial.Map(val => val.ToUpper());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.GetValueOrThrow().Should().Be("HELLO");
    }

    [Fact]
    public void Ensure_Should_ReturnFailure_WhenPredicateIsFalse()
    {
        // Arrange
        var initial = Result.Success(10);

        // Act
        var ensured = initial.Ensure(val => val > 20, TestError);

        // Assert
        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be(TestError);
    }

    [Fact]
    public async Task GenericResult_AsyncLambdasOnSyncResult_Should_ResolveToAsyncOverloads()
    {
        // Arrange
        var tapped = false;
        var initial = Result.Success(10);

        // Act
        var output = await initial
            .Tap(async _ =>
            {
                await Task.Yield();
                tapped = true;
            })
            .Ensure(async val =>
            {
                await Task.Yield();
                return val > 5;
            }, TestError)
            .Map(async val =>
            {
                await Task.Yield();
                return val * 3;
            })
            .Match(
                async val =>
                {
                    await Task.Yield();
                    return $"Async:{val}";
                },
                err => Task.FromResult(err.Code));

        // Assert
        tapped.Should().BeTrue();
        output.Should().Be("Async:30");
    }

    [Fact]
    public void Tap_Should_ExecuteAction_WhenSuccess()
    {
        // Arrange
        var executed = false;
        var initial = Result.Success("Data");

        // Act
        initial.Tap(_ => executed = true);

        // Assert
        executed.Should().BeTrue();
    }
}
