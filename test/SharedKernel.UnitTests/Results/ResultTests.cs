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
        var result = Result.Success();

        result.Value.Should().Be(Success.Instance);
    }

    [Fact]
    public void Failure_Should_CreateFailedResultWithError()
    {
        var result = Result.Failure(TestError);

        result.Value.Should().Be(TestError);
    }

    [Fact]
    public void Failure_Should_Throw_WhenErrorIsNone()
    {
        var act = () => Result.Failure(Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FailureT_Should_CreateFailedResultWithError()
    {
        var result = Result.Failure<int>(TestError);

        result.Value.Should().Be(TestError);
    }

    [Fact]
    public void FailureT_Should_Throw_WhenErrorIsNone()
    {
        var act = () => Result.Failure<int>(Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SuccessT_Should_ContainValue()
    {
        var result = Result.Success(42);

        result.Value.Should().Be(new Success<int>(42));
    }

    [Fact]
    public void Create_Should_ReturnSuccess_WhenValueIsNotNull()
    {
        var result = Result.Create("hello");

        result.Value.Should().Be(new Success<string>("hello"));
    }

    [Fact]
    public void Create_Should_ReturnNullValueError_WhenValueIsNull()
    {
        var result = Result.Create<string>(null);

        result.Value.Should().Be(Error.NullValue);
    }

    [Fact]
    public void SuccessAndFailure_Should_NotCollide_WhenTValueIsError()
    {
        var payload = new Error("Payload.Error", "Business payload");

        var success = Result.Success(payload);
        var failure = Result.Failure<Error>(TestError);

        (success switch
        {
            Success<Error>(var value) => $"success:{value.Code}",
            Error err => $"failure:{err.Code}"
        }).Should().Be("success:Payload.Error");

        (failure switch
        {
            Success<Error>(var value) => $"success:{value.Code}",
            Error err => $"failure:{err.Code}"
        }).Should().Be("failure:Test.Error");
    }

    [Fact]
    public void ImplicitConversions_Should_WorkForErrorAndValue()
    {
        Result nonGenericError = TestError;
        Result<int> genericError = TestError;
        Result<string> genericValue = "hello";
        Result<string> nullValue = (string?)null;

        nonGenericError.Value.Should().Be(TestError);
        genericError.Value.Should().Be(TestError);
        genericValue.Value.Should().Be(new Success<string>("hello"));
        nullValue.Value.Should().Be(Error.NullValue);
    }

    [Fact]
    public void UnionSwitchExpression_Should_MatchExhaustively()
    {
        Result nonGeneric = Result.Success();
        Result<int> success = Result.Success(42);
        Result<int> failure = TestError;

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

        nonGenericText.Should().Be("ok");
        successText.Should().Be("ok:42");
        failureText.Should().Be(TestError.Code);
    }
}
