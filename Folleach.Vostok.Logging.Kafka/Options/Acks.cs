namespace Folleach.Vostok.Logging.Kafka.Options;

public enum Acks
{
    None = 0,
    Leader = 1,
    All = -1
}

internal static class AcksMap
{
    public static Confluent.Kafka.Acks From(Acks? acks)
    {
        return acks switch
        {
            Acks.None => Confluent.Kafka.Acks.None,
            Acks.Leader => Confluent.Kafka.Acks.Leader,
            Acks.All => Confluent.Kafka.Acks.All,
            null => Confluent.Kafka.Acks.All,
            _ => throw new ArgumentOutOfRangeException($"unknow acks type: {acks}")
        };
    }
}
