using Confluent.Kafka;
using Folleach.Vostok.Logging.Kafka.Options;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;

namespace Folleach.Vostok.Logging.Kafka;

public class KafkaLog : ILog, IDisposable
{
    private readonly IProducer<string, KafkaLogEvent> producer;
    private readonly KafkaOptions options;
    private readonly Func<KafkaLogEvent, string> keySelector;
    private int monotonicCounter = 0;
    private long eventLost = 0;

    public KafkaLog(KafkaOptions options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = string.Join(",", options.BootstrapServers),
            LingerMs = options.Transport?.LingerMs,
            BatchSize = options.Transport?.BatchSize,
            BatchNumMessages = options.Transport?.BatchNumMessages,
            QueueBufferingMaxKbytes = options.Transport?.QueueBufferingMaxKbytes,
            QueueBufferingMaxMessages = options.Transport?.QueueBufferingMaxMessages,
            Acks = AcksMap.From(options.Transport?.Acks),
            CompressionType = CompressionMap.From(options.Transport?.Compression)
        };

        keySelector = options.KeySelector ?? (x => $"{x.Timestamp:O}-{x.Lt}");
        producer = new ProducerBuilder<string, KafkaLogEvent>(config)
            .SetValueSerializer(new KafkaLogEventSerde())
            .Build();
        this.options = options;
    }

    public long TotalEventsLost => eventLost;

    public ILog ForContext(string context) => new SourceContextWrapper(this, context);

    public bool IsEnabledFor(LogLevel level) => true;

    public void Log(LogEvent @event)
    {
        var kafkaLogEvent = new KafkaLogEvent(@event, Interlocked.Increment(ref monotonicCounter));
        try
        {
            var message = new Message<string, KafkaLogEvent>
            {
                Timestamp = new Timestamp(@event.Timestamp),
                Key = keySelector(kafkaLogEvent),
                Value = kafkaLogEvent
            };
            producer.Produce(options.Topic, message);
        }
        catch (Exception)
        {
            Interlocked.Increment(ref eventLost);
        }
    }

    public void Dispose()
    {
        producer.Dispose();
    }

    public void Flush()
    {
        producer.Flush();
    }
}
