using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.CoverageTests;

public class MessageTests
{
    [Fact]
    public void Constructor_SetsTopicAndPayload()
    {
        var msg = new Message("test/topic", "hello");

        Assert.Equal("test/topic", msg.Topic);
        Assert.Equal("hello", msg.Payload);
    }

    [Fact]
    public void Constructor_AllowsEmptyPayload()
    {
        var msg = new Message("topic", "");

        Assert.Equal("topic", msg.Topic);
        Assert.Equal("", msg.Payload);
    }

    [Fact]
    public void Constructor_AllowsNullPayload()
    {
        var msg = new Message("topic", null);

        Assert.Equal("topic", msg.Topic);
        Assert.Null(msg.Payload);
    }

    [Fact]
    public void Constructor_AllowsNullTopic()
    {
        var msg = new Message(null, "payload");

        Assert.Null(msg.Topic);
        Assert.Equal("payload", msg.Payload);
    }
}
