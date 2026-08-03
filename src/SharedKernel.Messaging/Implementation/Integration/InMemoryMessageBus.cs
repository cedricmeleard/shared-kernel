using System.Threading.Channels;
using Messaging.Abstractions.Integration;

namespace Messaging.Implementation.Integration;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly Channel<IIntegrationEvent> _channel = Channel.CreateUnbounded<IIntegrationEvent>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    public ChannelReader<IIntegrationEvent> Reader => _channel.Reader;

    public ValueTask PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent =>
        _channel.Writer.WriteAsync(integrationEvent, cancellationToken);

    Task IMessageBus.PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) =>
        PublishAsync(integrationEvent, cancellationToken).AsTask();
}
