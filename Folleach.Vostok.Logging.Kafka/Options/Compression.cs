using Confluent.Kafka;

namespace Folleach.Vostok.Logging.Kafka.Options;

public enum Compression
{
    None,
    Gzip,
    Snappy,
    Lz4,
    Zstd
}

internal static class CompressionMap
{
    public static CompressionType? From(Compression? type)
    {
        return type switch
        {
            Compression.None => CompressionType.None,
            Compression.Gzip => CompressionType.Gzip,
            Compression.Snappy => CompressionType.Snappy,
            Compression.Lz4 => CompressionType.Lz4,
            Compression.Zstd => CompressionType.Zstd,
            null => CompressionType.Zstd,
            _ => throw new ArgumentOutOfRangeException($"unknow compression type: {type}")
        };
    }
}
