namespace SharedKernel.Messaging.Abstractions.Events;

public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent,
        CancellationToken cancellationToken = default);

    Task PublishAsync<TEvent>(TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}