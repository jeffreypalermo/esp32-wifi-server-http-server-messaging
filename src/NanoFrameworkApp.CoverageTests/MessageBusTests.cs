using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.CoverageTests;

public class MessageBusTests
{
    [Fact]
    public void Subscribe_AddsHandlerForTopic()
    {
        var bus = new MessageBus();

        bus.Subscribe("test", msg => { });

        Assert.Equal(1, bus.SubscriberCount("test"));
    }

    [Fact]
    public void Subscribe_MultipleTimes_IncrementsCount()
    {
        var bus = new MessageBus();

        bus.Subscribe("test", msg => { });
        bus.Subscribe("test", msg => { });
        bus.Subscribe("test", msg => { });

        Assert.Equal(3, bus.SubscriberCount("test"));
    }

    [Fact]
    public void Subscribe_DifferentTopics_TrackedSeparately()
    {
        var bus = new MessageBus();

        bus.Subscribe("alpha", msg => { });
        bus.Subscribe("beta", msg => { });

        Assert.Equal(1, bus.SubscriberCount("alpha"));
        Assert.Equal(1, bus.SubscriberCount("beta"));
    }

    [Fact]
    public void SubscriberCount_UnknownTopic_ReturnsZero()
    {
        var bus = new MessageBus();

        Assert.Equal(0, bus.SubscriberCount("nonexistent"));
    }

    [Fact]
    public void Publish_DeliversMessageToSubscriber()
    {
        var bus = new MessageBus();
        Message received = null;

        bus.Subscribe("events", msg => received = msg);
        bus.Publish(new Message("events", "data"));

        Assert.NotNull(received);
        Assert.Equal("events", received.Topic);
        Assert.Equal("data", received.Payload);
    }

    [Fact]
    public void Publish_DeliversToAllSubscribers()
    {
        var bus = new MessageBus();
        int callCount = 0;

        bus.Subscribe("topic", msg => callCount++);
        bus.Subscribe("topic", msg => callCount++);
        bus.Subscribe("topic", msg => callCount++);

        bus.Publish(new Message("topic", "x"));

        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        var bus = new MessageBus();

        var ex = Record.Exception(() => bus.Publish(new Message("nobody", "data")));

        Assert.Null(ex);
    }

    [Fact]
    public void Publish_OnlyNotifiesMatchingTopic()
    {
        var bus = new MessageBus();
        bool alphaReceived = false;
        bool betaReceived = false;

        bus.Subscribe("alpha", msg => alphaReceived = true);
        bus.Subscribe("beta", msg => betaReceived = true);

        bus.Publish(new Message("alpha", ""));

        Assert.True(alphaReceived);
        Assert.False(betaReceived);
    }

    [Fact]
    public void Publish_IsThreadSafe()
    {
        var bus = new MessageBus();
        int counter = 0;
        var lockObj = new object();

        bus.Subscribe("concurrent", msg =>
        {
            lock (lockObj) { counter++; }
        });

        var threads = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    bus.Publish(new Message("concurrent", ""));
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        Assert.Equal(1000, counter);
    }

    [Fact]
    public void Subscribe_IsThreadSafe()
    {
        var bus = new MessageBus();

        var threads = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 50; j++)
                {
                    bus.Subscribe("topic", msg => { });
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        Assert.Equal(500, bus.SubscriberCount("topic"));
    }
}
