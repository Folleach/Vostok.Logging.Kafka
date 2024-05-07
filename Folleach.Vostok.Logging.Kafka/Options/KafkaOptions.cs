namespace Folleach.Vostok.Logging.Kafka.Options;

public class KafkaOptions
{
    public string Topic { get; }
    public string[] BootstrapServers { get; }
    public TransportKafkaOptions? Transport { get; init; }
    /// <summary>
    /// Requires for identify log event in kafka.<br/>
    /// By defaut id is "<b>timestamp-lt</b>"<br/>
    /// It's should be fine for one log producer.<br/>
    /// If you have multiple replicas or applications, it's recommended to override selector with the name of replica or/and application
    /// </summary>
    public Func<KafkaLogEvent, string>? KeySelector { get; init; }

    public KafkaOptions(string topic, string[] bootstrapServers, Func<KafkaLogEvent, string>? keySelector = null)
    {
        Topic = topic;
        BootstrapServers = bootstrapServers;
        KeySelector = keySelector;
    }
}
