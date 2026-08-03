using MediatR;
using Messaging.Abstractions.Events;

namespace Messaging.Implementation;

public sealed class EventPublisher(IPublisher publisher) : IEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var notification = DomainEventNotification.Create(domainEvent);
        return publisher.Publish(notification, cancellationToken);
    }

    public Task PublishAsync<TEvent>(TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent =>
        publisher.Publish(new DomainEventNotification<TEvent>(domainEvent), cancellationToken);
}

internal abstract class DomainEventNotification
{
    public static INotification Create(IDomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}

internal sealed class DomainEventNotification<TEvent>(TEvent domainEvent)
    : DomainEventNotification, INotification
    where TEvent : IDomainEvent
{
    public TEvent DomainEvent
    {
        get;
    } = domainEvent;
}