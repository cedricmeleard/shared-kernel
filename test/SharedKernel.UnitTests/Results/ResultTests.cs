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
        result.Value.Should().Be(Success.Instance);
    }

    [Fact]
    public void Failure_Should_CreateFailedResultWithError()
    {
        // Act
        var result = Result.Failure(TestError);

        // Assert
        result.Value.Should().Be(TestError);
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
        result.Value.Should().Be(new Success<int>(42));
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
        nonGenericError.Value.Should().Be(TestError);
        genericError.Value.Should().Be(TestError);
        genericValue.Value.Should().Be(new Success<string>("hello"));
        nullValue.Value.Should().Be(Error.NullValue);
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
    public void Bind_Should_ChainOperation_WhenSuccess()
    {
        // Arrange
        var initial = Result.Success(5);

        // Act
        var bound = initial.Bind(val => Result.Success(val * 2));

        // Assert
        bound.Value.Should().Be(new Success<int>(10));
    }

    [Fact]
    public void Bind_Should_ReturnFailure_WhenInitialIsFailure()
    {
        // Arrange
        var initial = Result.Failure<int>(TestError);

        // Act
        var bound = initial.Bind(val => Result.Success(val * 2));

        // Assert
        bound.Value.Should().Be(TestError);
    }

    [Fact]
    public async Task AsyncRopPipeline_Should_ChainCleanlyAndEndWithSwitch()
    {
        // Arrange
        var tapped = false;

        // Act
        var output = await Task.FromResult(Result.Success())
            .Bind(() => Task.FromResult(Result.Success(10)))
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
            }) switch
        {
            Success<int>(var val) => $"Done:{val}",
            Error err => err.Code
        };

        // Assert
        tapped.Should().BeTrue();
        output.Should().Be("Done:30");
    }

    [Fact]
    public void Map_Should_TransformValue_WhenSuccess()
    {
        // Arrange
        var initial = Result.Success("hello");

        // Act
        var mapped = initial.Map(val => val.ToUpper());

        // Assert
        mapped.Value.Should().Be(new Success<string>("HELLO"));
    }

    [Fact]
    public void Ensure_Should_ReturnFailure_WhenPredicateIsFalse()
    {
        // Arrange
        var initial = Result.Success(10);

        // Act
        var ensured = initial.Ensure(val => val > 20, TestError);

        // Assert
        ensured.Value.Should().Be(TestError);
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
