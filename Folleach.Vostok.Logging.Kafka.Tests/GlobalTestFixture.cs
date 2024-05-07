using Confluent.Kafka;
using NUnit.Framework;

namespace Folleach.Vostok.Logging.Kafka.Tests;

[SetUpFixture]
public class GlobalTestFixture
{
    private IAsyncDisposable? containers;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        containers = await Docker.RunContainers("env/docker-compose.yaml");

        var attempt = 10;
        while (true)
        {
            var config = new ProducerConfig { BootstrapServers = Constants.BootstrapServer };
            using var adminClient = new AdminClientBuilder(config).Build();
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                break;
            }
            catch (Exception)
            {
                if (attempt-- > 0)
                    continue;
                throw;
            }
        }
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await (containers?.DisposeAsync() ?? ValueTask.CompletedTask);
    }
}
