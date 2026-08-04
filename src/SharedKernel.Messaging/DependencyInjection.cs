using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Messaging.Abstractions;
using SharedKernel.Messaging.Abstractions.Events;
using SharedKernel.Messaging.Abstractions.Integration;
using SharedKernel.Messaging.Behaviors;
using SharedKernel.Messaging.Implementation;
using SharedKernel.Messaging.Implementation.Integration;

namespace SharedKernel.Messaging;

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