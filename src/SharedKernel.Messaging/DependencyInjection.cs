using System.Reflection;
using Messaging.Abstractions;
using Messaging.Abstractions.Events;
using Messaging.Abstractions.Integration;
using Messaging.Behaviors;
using Messaging.Implementation;
using Messaging.Implementation.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddTransient<ISender, Sender>();
        services.AddTransient<IEventPublisher, EventPublisher>();

        // Async MessageBus Abstraction & In-Memory Channel Implementation
        var messageBus = new InMemoryMessageBus();
        services.AddSingleton(messageBus);
        services.AddSingleton<IMessageBus>(messageBus);
        services.AddHostedService<ChannelMessageBusHostedService>();

        // Register Integration Event Handlers from Assemblies
        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                    .Select(i => new { InterfaceType = i, ImplementationType = t }));

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.InterfaceType, handler.ImplementationType);
            }
        }

        return services;
    }
}