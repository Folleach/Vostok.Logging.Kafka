using Vostok.Logging.Abstractions;

namespace Folleach.Vostok.Logging.Kafka;

public class KafkaLogEvent
{
    public LogLevel level { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MessageTemplate { get; set; }
    public IReadOnlyDictionary<string, object>? Properties { get; set; }
    public Exception? Exception { get; set; }
    public int Lt { get; set; }

    public KafkaLogEvent()
    {
    }

    public KafkaLogEvent(LogEvent @event, int lt)
    {
        level = @event.Level;
        Timestamp = @event.Timestamp;
        MessageTemplate = @event.MessageTemplate;
        Properties = @event.Properties;
        Exception = @event.Exception;
        Lt = lt;
    }
}
