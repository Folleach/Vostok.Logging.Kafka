using System.Text.Json;
using Folleach.Vostok.Logging.Kafka.Options;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;

namespace Folleach.Vostok.Logging.Kafka.Tests;

[TestFixture]
public class KafkaLogTests
{
    [Test]
    public void Simple_InfoLog()
    {
        var topic = Guid.NewGuid().ToString();
        using var log = new KafkaLog(new KafkaOptions(topic, [Constants.BootstrapServer]));

        log.Info("hello");
        log.Flush();

        var result = LogEventConsumer.Consume(topic, 1).FirstOrDefault()!;
        Assert.AreEqual("hello", result.Message.Value.MessageTemplate);
        Assert.AreEqual(1, result.Message.Value.Lt);
        Assert.AreEqual(LogLevel.Info, result.Message.Value.level);
        Assert.AreEqual(null, result.Message.Value.Exception);
        Assert.AreEqual(null, result.Message.Value.Properties);
    }

    [Test]
    public void WithProperties_InfoLog()
    {
        var topic = Guid.NewGuid().ToString();
        using var log = new KafkaLog(new KafkaOptions(topic, [Constants.BootstrapServer]));

        log.WithProperty("some", "prop value").Info("hello");
        log.Flush();

        var result = LogEventConsumer.Consume(topic, 1).FirstOrDefault()!;
        Assert.AreEqual("hello", result.Message.Value.MessageTemplate);
        Assert.AreEqual(1, result.Message.Value.Lt);
        Assert.AreEqual(LogLevel.Info, result.Message.Value.level);
        Assert.AreEqual(null, result.Message.Value.Exception);
        Assert.AreEqual(1, result.Message.Value.Properties?.Count);
        Assert.AreEqual("prop value", JsonSerializer.Deserialize<string>((JsonElement)result.Message.Value.Properties!["some"]));
    }

    [Test]
    public void WithSourceContext_WarnLog()
    {
        var topic = Guid.NewGuid().ToString();
        using var log = new KafkaLog(new KafkaOptions(topic, [Constants.BootstrapServer]));

        log.ForContext("my context").Warn("hello");
        log.Flush();

        var result = LogEventConsumer.Consume(topic, 1).FirstOrDefault()!;
        Assert.AreEqual("hello", result.Message.Value.MessageTemplate);
        Assert.AreEqual(1, result.Message.Value.Lt);
        Assert.AreEqual(LogLevel.Warn, result.Message.Value.level);
        Assert.AreEqual(null, result.Message.Value.Exception);
        Assert.AreEqual(1, result.Message.Value.Properties?.Count);
        Assert.AreEqual(new string[] { "my context" }, JsonSerializer.Deserialize<string[]>((JsonElement)result.Message.Value.Properties![WellKnownProperties.SourceContext]));
    }

    [Test]
    public void WithOperationContext_ErrorLog()
    {
        var topic = Guid.NewGuid().ToString();
        using var log = new KafkaLog(new KafkaOptions(topic, [Constants.BootstrapServer]));
        using (new OperationContextToken("op1"))
        {
            using (new OperationContextToken("op2"))
                log.WithOperationContext().Error("make operation");
        }

        log.ForContext("my context").Warn("hello");
        log.Flush();

        var result = LogEventConsumer.Consume(topic, 1).FirstOrDefault()!;
        Assert.AreEqual("make operation", result.Message.Value.MessageTemplate);
        Assert.AreEqual(1, result.Message.Value.Lt);
        Assert.AreEqual(LogLevel.Error, result.Message.Value.level);
        Assert.AreEqual(null, result.Message.Value.Exception);
        Assert.AreEqual(1, result.Message.Value.Properties?.Count);
        Assert.AreEqual(new string[] { "op1", "op2" }, JsonSerializer.Deserialize<string[]>((JsonElement)result.Message.Value.Properties![WellKnownProperties.OperationContext]));
    }
}
