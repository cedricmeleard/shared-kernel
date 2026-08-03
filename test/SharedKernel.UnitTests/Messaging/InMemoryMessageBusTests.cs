using FluentAssertions;
using Messaging.Abstractions.Integration;
using Messaging.Implementation.Integration;
using Xunit;

namespace SharedKernel.UnitTests.Messaging;

public record TestIntegrationEvent(string Payload) : IntegrationEvent;

public class InMemoryMessageBusTests
{
    [Fact]
    public async Task PublishAsync_Should_WriteIntegrationEventToChannel()
    {
        // Arrange
        var messageBus = new InMemoryMessageBus();
        var integrationEvent = new TestIntegrationEvent("Hello Channel");

        // Act
        await messageBus.PublishAsync(integrationEvent);

        // Assert
        var itemRead = await messageBus.Reader.ReadAsync();
        itemRead.Should().Be(integrationEvent);
        ((TestIntegrationEvent)itemRead).Payload.Should().Be("Hello Channel");
    }
}
