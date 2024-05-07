using Confluent.Kafka;

namespace Folleach.Vostok.Logging.Kafka.Tests;

public class LogEventConsumer
{
    public static IEnumerable<ConsumeResult<string, KafkaLogEvent>> Consume(string topic, int count)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = Constants.BootstrapServer,
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, KafkaLogEvent>(config)
            .SetValueDeserializer(new KafkaLogEventSerde())
            .Build();
        consumer.Subscribe(topic);

        for (var i = 0; i < count; i++)
            yield return consumer.Consume();

        consumer.Close();
    }
}
