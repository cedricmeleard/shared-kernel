using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging.Abstractions.Integration;

namespace SharedKernel.Messaging.Implementation.Integration;

public sealed class ChannelMessageBusHostedService(
    InMemoryMessageBus messageBus,
    IServiceScopeFactory scopeFactory,
    ILogger<ChannelMessageBusHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Async MessageBus processing channel started.");

        await foreach (var integrationEvent in messageBus.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var eventType = integrationEvent.GetType();
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    if (handler is null)
                    {
                        continue;
                    }

                    var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync));
                    if (handleMethod is not null)
                    {
                        var task = (Task)handleMethod.Invoke(handler, [integrationEvent, stoppingToken])!;
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing async integration event {EventType} ({EventId})",
                    integrationEvent.GetType().Name, integrationEvent.EventId);
            }
        }
    }
}
