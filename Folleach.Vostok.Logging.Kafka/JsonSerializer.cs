using System.Text.Json;
using Confluent.Kafka;

namespace Folleach.Vostok.Logging.Kafka;

public class KafkaLogEventSerde : ISerializer<KafkaLogEvent>, IDeserializer<KafkaLogEvent>
{
    public KafkaLogEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return isNull ? null! : JsonSerializer.Deserialize<KafkaLogEvent>(data)!;
    }

    public byte[] Serialize(KafkaLogEvent data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}
