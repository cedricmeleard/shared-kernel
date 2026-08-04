using FluentAssertions;
using SharedKernel.Messaging.Implementation.Integration.Postgres;
using Xunit;

namespace SharedKernel.UnitTests.Messaging;

public class PostgresMessageBusOptionsTests
{
    [Fact]
    public void Options_Should_HaveCorrectDefaults()
    {
        // Act
        var options = new PostgresMessageBusOptions();

        // Assert
        options.SchemaName.Should().Be("messaging");
        options.TableName.Should().Be("outbox_messages");
        options.BatchSize.Should().Be(50);
        options.PollingInterval.Should().Be(TimeSpan.FromSeconds(2));
        options.ConnectionString.Should().BeNull();
    }
}
