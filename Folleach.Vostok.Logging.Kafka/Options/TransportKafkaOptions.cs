namespace Folleach.Vostok.Logging.Kafka.Options;

/// <summary>
/// This options maps 1 to 1 to ProducerConfig from<br/>
/// https://github.com/confluentinc/confluent-kafka-dotnet
/// </summary>
public class TransportKafkaOptions
{
    public Acks Acks { get; init; } = Acks.Leader;
    public Compression Compression { get; init; } = Compression.Zstd;
    public int? QueueBufferingMaxMessages { get; init; } = null;
    public int? QueueBufferingMaxKbytes { get; init; } = null;
    public int? BatchNumMessages { get; init; } = null;
    public int? BatchSize { get; init; } = null;
    public int? LingerMs { get; init; } = null;
}
