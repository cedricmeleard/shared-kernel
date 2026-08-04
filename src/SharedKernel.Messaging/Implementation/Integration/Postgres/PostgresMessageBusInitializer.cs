using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace SharedKernel.Messaging.Implementation.Integration.Postgres;

public class PostgresMessageBusInitializer(
    NpgsqlDataSource dataSource,
    IOptions<PostgresMessageBusOptions> options,
    ILogger<PostgresMessageBusInitializer> logger) : IHostedService
{
    private readonly PostgresMessageBusOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing PostgresMessageBus schema '{Schema}' and table '{Table}'...",
            _options.SchemaName, _options.TableName);

        var sql = $"""
            CREATE SCHEMA IF NOT EXISTS {_options.SchemaName};

            CREATE TABLE IF NOT EXISTS {_options.SchemaName}.{_options.TableName} (
                id UUID PRIMARY KEY,
                event_type VARCHAR(500) NOT NULL,
                payload TEXT NOT NULL,
                occurred_on_utc TIMESTAMPTZ NOT NULL,
                processed_at_utc TIMESTAMPTZ NULL,
                error_message TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_{_options.TableName}_unprocessed
            ON {_options.SchemaName}.{_options.TableName} (occurred_on_utc)
            WHERE processed_at_utc IS NULL;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation("PostgresMessageBus schema '{Schema}' initialized successfully.", _options.SchemaName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
