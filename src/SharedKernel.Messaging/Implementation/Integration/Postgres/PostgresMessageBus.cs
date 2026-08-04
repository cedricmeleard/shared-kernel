using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedKernel.Messaging.Abstractions.Integration;

namespace SharedKernel.Messaging.Implementation.Integration.Postgres;

public class PostgresMessageBus(
    NpgsqlDataSource dataSource,
    IOptions<PostgresMessageBusOptions> options) : IMessageBus
{
    private readonly PostgresMessageBusOptions _options = options.Value;

    public async Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
    {
        var sql = $"""
            INSERT INTO {_options.SchemaName}.{_options.TableName}
                (id, event_type, payload, occurred_on_utc)
            VALUES (@Id, @EventType, @Payload, @OccurredOnUtc);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        var eventType = integrationEvent.GetType().AssemblyQualifiedName
                        ?? integrationEvent.GetType().FullName!;
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());

        command.Parameters.AddWithValue("Id", Guid.NewGuid());
        command.Parameters.AddWithValue("EventType", eventType);
        command.Parameters.AddWithValue("Payload", payload);
        command.Parameters.AddWithValue("OccurredOnUtc", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
