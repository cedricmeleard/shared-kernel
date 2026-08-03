using FluentAssertions;
using Results;
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
    public void SuccessT_Should_ContainValue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
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
        bound.Value.Should().Be(10);
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
    public void Map_Should_TransformValue_WhenSuccess()
    {
        // Arrange
        var initial = Result.Success("hello");

        // Act
        var mapped = initial.Map(val => val.ToUpper());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("HELLO");
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
