using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedKernel.Messaging.Abstractions.Integration;

namespace SharedKernel.Messaging.Implementation.Integration.Postgres;

public static class PostgresMessageBusExtensions
{
    public static IServiceCollection AddPostgresMessageBus(
        this IServiceCollection services,
        Action<PostgresMessageBusOptions>? configure = null)
    {
        var optionsBuilder = services.AddOptions<PostgresMessageBusOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresMessageBusOptions>>().Value;
            var connectionString = options.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionString must be configured for PostgresMessageBus.");
            }

            return NpgsqlDataSource.Create(connectionString);
        });

        services.AddSingleton<IMessageBus, PostgresMessageBus>();
        services.AddHostedService<PostgresMessageBusInitializer>();
        services.AddHostedService<PostgresMessageBusHostedService>();

        return services;
    }
}
