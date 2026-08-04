using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedKernel.Messaging.Abstractions.Integration;

namespace SharedKernel.Messaging.Implementation.Integration.Postgres;

public class PostgresMessageBusHostedService(
    NpgsqlDataSource dataSource,
    IServiceScopeFactory scopeFactory,
    IOptions<PostgresMessageBusOptions> options,
    ILogger<PostgresMessageBusHostedService> logger) : BackgroundService
{
    private readonly PostgresMessageBusOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting PostgresMessageBus worker (Schema: '{Schema}', Interval: {Interval}s)...",
            _options.SchemaName, _options.PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing outbox messages in PostgresMessageBus");
            }

            await Task.Delay(_options.PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        var selectSql = $"""
            SELECT id, event_type, payload
            FROM {_options.SchemaName}.{_options.TableName}
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_on_utc ASC
            LIMIT @BatchSize;
            """;

        var updateSql = $"""
            UPDATE {_options.SchemaName}.{_options.TableName}
            SET processed_at_utc = @ProcessedAtUtc, error_message = @ErrorMessage
            WHERE id = @Id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(selectSql, connection);
        command.Parameters.AddWithValue("BatchSize", _options.BatchSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var messagesToProcess = new List<(Guid Id, string EventType, string Payload)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            messagesToProcess.Add((
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }
        await reader.CloseAsync();

        if (messagesToProcess.Count == 0)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();

        foreach (var (id, eventTypeStr, payload) in messagesToProcess)
        {
            string? errorMessage = null;
            try
            {
                var eventType = Type.GetType(eventTypeStr);
                if (eventType is null)
                {
                    logger.LogWarning("Unknown event type '{EventType}' for message {MessageId}", eventTypeStr, id);
                    errorMessage = $"Unknown event type '{eventTypeStr}'";
                }
                else
                {
                    var integrationEvent = (IIntegrationEvent?)JsonSerializer.Deserialize(payload, eventType);
                    if (integrationEvent is null)
                    {
                        errorMessage = "Failed to deserialize payload";
                    }
                    else
                    {
                        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var handlers = scope.ServiceProvider.GetServices(handlerType);

                        foreach (var handler in handlers)
                        {
                            var method = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync));
                            if (method is not null)
                            {
                                var task = (Task?)method.Invoke(handler, [integrationEvent, cancellationToken]);
                                if (task is not null)
                                {
                                    await task;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle integration message {MessageId}", id);
                errorMessage = ex.Message;
            }

            await using var updateCmd = new NpgsqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("Id", id);
            updateCmd.Parameters.AddWithValue("ProcessedAtUtc", DateTime.UtcNow);
            updateCmd.Parameters.AddWithValue("ErrorMessage", (object?)errorMessage ?? DBNull.Value);
            await updateCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
