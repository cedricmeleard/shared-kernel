using FluentAssertions;
using FluentValidation;
using SharedKernel.Messaging.Behaviors;
using SharedKernel.Results;
using Xunit;

namespace SharedKernel.UnitTests.Messaging;

public class ValidationPipelineBehaviorTests
{
    public record TestCommand(string Name) : SharedKernel.Messaging.Abstractions.Commands.ICommand;

    public record TestGenericCommand(string Name) : SharedKernel.Messaging.Abstractions.Commands.ICommand<int>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }

    private sealed class TestGenericCommandValidator : AbstractValidator<TestGenericCommand>
    {
        public TestGenericCommandValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResult_WhenValidationFails_ForNonGenericCommand()
    {
        // Arrange
        var behavior = new ValidationPipelineBehavior<TestCommand, Result>([new TestCommandValidator()]);
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(
            new TestCommand(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.Value.Should().Be(new Error("Validation.Error", "Name is required"));
    }

    [Fact]
    public async Task Handle_Should_ReturnFailureResultT_WhenValidationFails_ForGenericCommand()
    {
        // Arrange
        var behavior = new ValidationPipelineBehavior<TestGenericCommand, Result<int>>([new TestGenericCommandValidator()]);
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(
            new TestGenericCommand(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success(123));
            },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.Value.Should().Be(new Error("Validation.Error", "Name is required"));
    }

    [Fact]
    public async Task Handle_Should_InvokeNext_WhenValidationSucceeds()
    {
        // Arrange
        var behavior = new ValidationPipelineBehavior<TestGenericCommand, Result<int>>([new TestGenericCommandValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestGenericCommand("Valid"),
            _ => Task.FromResult(Result.Success(123)),
            CancellationToken.None);

        // Assert
        result.Value.Should().Be(new Success<int>(123));
    }
}
