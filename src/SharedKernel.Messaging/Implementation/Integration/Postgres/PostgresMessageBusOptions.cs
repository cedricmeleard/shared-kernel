namespace SharedKernel.Messaging.Implementation.Integration.Postgres;

public class PostgresMessageBusOptions
{
    public string? ConnectionString { get; set; }
    public string SchemaName { get; set; } = "messaging";
    public string TableName { get; set; } = "outbox_messages";
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);
    public int BatchSize { get; set; } = 50;
}
