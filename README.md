Asynchronous Kafka sink for [Vostok Logging](https://vostok.gitbook.io/logging) infrastructure.  

This is simple and fast implementation just for logging.  
If you want to use full features of Vostok infrastructure, probably you'd better use [Hercules](https://github.com/vostok/hercules).  

## Usage

```cs
var log = new KafkaLog(new KafkaOptions(topic, ["localhost:9092"]));

// lifetime of log

log.Flush();
log.Dispose();
```

## Recomendations

### Override key selector

Default id of log event in kafka is `Timestamp-Lt`.  
`Lt` is a monotonously increasing cyclic number.  

If you have multiple replicas or applications, it's recommended to override key selector with the name of replica or/and application.  
This is requires to understand log event difference between:
- log with id `2024-05-07T01:02:03-100` from `service1`
- log with id `2024-05-07T01:02:03-100` from `service2`

```cs
var log = new KafkaLog(new KafkaOptions("topic", ["localhost:9092"])
{
    KeySelector = x => $"{x.Timestamp:O}-{x.Lt}-{x.Properties?["service"] ?? "unknown"}",
});
```

## Peformance

Tested with:

| | CPU cores | CPU speed | RAM | Disk | Kafka |
| --- | --- | --- | --- | --- | --- |
| Log producer | 4/8 | 3.7 GHz | 32 GB | - | [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka) v2.4.0 |
| Kafka server | 20/40 | 2.9 GHz | 157 GB | HDD, 4 TB, 5400 rpm | 3 nodes of [confluentinc/cp-kafka](https://hub.docker.com/r/confluentinc/cp-kafka) v7.0.1 |

Network between machines: 1 Gbit/s

### Terms

#### Default settings

Server:
```
Partitions = 3
Replication factor = 1
```

Client
```
Acks = Leader
Compression = Zstd
```

Other settings are default values by kafka or client

#### Log tradeoff

The approximate time spent on calling
```cs
log.Info("some");
```
If you have a web service that prints the request path, you can add this time to latency.  

### Bench #1

Parameters:
- defult settings  
- 1 producer thread  
- 256 bytes log message  

#### Result

| Time | Events count | RPS | Lost events | Log tradeoff |
| --- | --- | --- | --- | --- |
| 00:00:10.0173258 | 3729396 | 386623 | 4258 | 2600 ns |
| 00:00:11.0206983 | 4134691 | 405295 | 4258 | 2500 ns |
| 00:00:12.0210021 | 4548473 | 413782 | 4258 | 2400 ns |
| 00:00:13.0211998 | 4942791 | 394318 | 4258 | 2500 ns |
| 00:00:14.0214208 | 5349694 | 406903 | 4258 | 2500 ns |
| 00:00:15.0240757 | 5739572 | 389878 | 4258 | 2600 ns |
| 00:00:16.0243038 | 6160076 | 420504 | 4258 | 2400 ns |
| 00:00:17.0245054 | 6569357 | 409281 | 4258 | 2400 ns |
| 00:00:18.0246864 | 6994773 | 425416 | 4258 | 2400 ns |
| 00:00:19.0249027 | 7410307 | 415534 | 4258 | 2400 ns |

There are several lost events, because small messages quickly overflowed the queue at the beginning.  
I don't think anyone will have that many logs.  
In any case, you can set up a queue size  
```cs
var log = new KafkaLog(new KafkaOptions("topic", ["localhost:9092"])
{
    Transport = new TransportKafkaOptions()
    {
        QueueBufferingMaxMessages = 100500
    }
});
```

### Bench #2

Parameters:
- defult settings  
- 1 producer thread  
- 2 KBytes log message  

#### Result

| Time | Events count | RPS | Lost events | Log tradeoff |
| --- | --- | --- | --- | --- |
| 00:00:10.0246786 | 2296452 | 253857 | 0 | 3900 ns |
| 00:00:11.0251509 | 2549875 | 253423 | 0 | 3900 ns |
| 00:00:12.0254687 | 2796446 | 246571 | 0 | 4100 ns |
| 00:00:13.0256997 | 3042039 | 245593 | 0 | 4100 ns |
| 00:00:14.0258961 | 3294887 | 252848 | 0 | 4000 ns |
| 00:00:15.0261386 | 3542430 | 247543 | 0 | 4000 ns |
| 00:00:16.0263851 | 3789320 | 246890 | 0 | 4100 ns |
| 00:00:17.0266416 | 4042493 | 253173 | 0 | 3900 ns |
| 00:00:18.0268734 | 4296709 | 254216 | 0 | 3900 ns |
| 00:00:19.0271659 | 4548418 | 251709 | 0 | 4000 ns |

### Bench #3

Parameters:
- defult settings  
- 1 producer thread  
- 16 KBytes log message  

| Time | Events count | RPS | Lost events | Log tradeoff |
| --- | --- | --- | --- | --- |
| 00:00:10.0207989 | 667539  | 70048  | 0 | 14300 ns |
| 00:00:11.0210313 | 739711  | 72172  | 0 | 13900 ns |
| 00:00:12.0213534 | 811211  | 71500  | 0 | 14000 ns |
| 00:00:13.0217162 | 882952  | 71741  | 0 | 13900 ns |
| 00:00:14.0221874 | 955325  | 72373  | 0 | 13800 ns |
| 00:00:15.0224457 | 1025999 | 70674  | 0 | 14100 ns |
| 00:00:16.0228297 | 1094652 | 68653  | 0 | 14600 ns |
| 00:00:17.0234830 | 1162128 | 67476  | 0 | 14800 ns |
| 00:00:18.0202662 | 1230920 | 68792  | 0 | 14500 ns |
| 00:00:19.0239804 | 1298341 | 67421  | 0 | 14800 ns |
