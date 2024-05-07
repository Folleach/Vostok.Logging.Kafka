using System.Diagnostics;
using Folleach.Vostok.Logging.Kafka;
using Folleach.Vostok.Logging.Kafka.Options;
using Vostok.Logging.Abstractions;

// config
var parallel = 1;
var logMessageSizeInBytes = 256;

var cts = new CancellationTokenSource();
var eventsCount = 0L;
var message = new string('a', logMessageSizeInBytes);

var log = new KafkaLog(new KafkaOptions(topic: "folleach-logging-kafka-bench", bootstrapServers: ["192.168.1.5:9092", "192.168.1.5:9093", "192.168.1.5:9094"])
{
    KeySelector = x => $"{x.Timestamp:O}-{x.Lt}-{x.Properties?["service"] ?? "unknown"}",
});

Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var time = Stopwatch.StartNew();
var printer = Task.Run(async () =>
{
    var old = eventsCount;
    while (!cts.Token.IsCancellationRequested)
    {
        var current = eventsCount;
        var rps = current - old;
        var oneCallTradeoff = TimeSpan.FromSeconds(1) / (rps == 0 ? 1 : rps);
        Console.WriteLine($"{time.Elapsed}\tevent count: {current}\trps: {rps}\tlost: {log.TotalEventsLost}\tone call time: {oneCallTradeoff.TotalNanoseconds} ns");
        old = current;
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

var threads = Enumerable.Range(0, parallel).Select(i => new Thread(() =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        log.Info(message);
        Interlocked.Increment(ref eventsCount);
    }
})
{
    Name = $"generator #{i}"
}).ToArray();

foreach (var thread in threads)
    thread.Start();

foreach (var thread in threads)
    thread.Join();

var flushTimer = Stopwatch.StartNew();
log.Flush();
flushTimer.Stop();

var disposeTimer = Stopwatch.StartNew();
log.Dispose();
disposeTimer.Stop();

await printer;

Console.WriteLine($"log flush time: {flushTimer.Elapsed}");
Console.WriteLine($"log dispose time: {disposeTimer.Elapsed}");
