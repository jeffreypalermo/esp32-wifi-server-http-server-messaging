using NanoFrameworkApp.Hardware;
using NanoFrameworkApp.Messaging;
using NanoFrameworkApp.Workers;
using NanoFrameworkApp;

namespace NanoFrameworkApp.CoverageTests;

public class LedWorkerTests
{
    private class FakeLedController : ILedController
    {
        public int TurnOnCount { get; private set; }
        public int TurnOffCount { get; private set; }
        public bool IsOn { get; private set; }

        public void TurnOn()
        {
            IsOn = true;
            TurnOnCount++;
        }

        public void TurnOff()
        {
            IsOn = false;
            TurnOffCount++;
        }
    }

    [Fact]
    public void Constructor_DoesNotStartWorker()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();

        var worker = new LedWorker(led, bus, new DeviceStatus());

        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void Name_ReturnsLedWorker()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();

        var worker = new LedWorker(led, bus, new DeviceStatus());

        Assert.Equal("LedWorker", worker.Name);
    }

    [Fact]
    public void Start_SetsIsRunning()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());

        worker.Start();

        Assert.True(worker.IsRunning);
    }

    [Fact]
    public void Start_SubscribesToLedFlashTopic()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());

        worker.Start();

        Assert.Equal(1, bus.SubscriberCount("led/flash"));
    }

    [Fact]
    public void Stop_ClearsIsRunning()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        worker.Stop();

        Assert.False(worker.IsRunning);
    }

    [Fact]
    public void Stop_TurnsOffLed()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        worker.Stop();

        Assert.False(led.IsOn);
        Assert.Equal(1, led.TurnOffCount);
    }

    [Fact]
    public void FlashMessage_WithPayload3_FlashesLed3Times()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("led/flash", "3"));

        Assert.Equal(3, led.TurnOnCount);
        Assert.Equal(3, led.TurnOffCount);
        Assert.Equal(3, worker.FlashCount);
    }

    [Fact]
    public void FlashMessage_WithPayload1_FlashesLed1Time()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("led/flash", "1"));

        Assert.Equal(1, led.TurnOnCount);
        Assert.Equal(1, led.TurnOffCount);
        Assert.Equal(1, worker.FlashCount);
    }

    [Fact]
    public void FlashMessage_WithPayload0_DoesNotFlash()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("led/flash", "0"));

        Assert.Equal(0, led.TurnOnCount);
        Assert.Equal(0, led.TurnOffCount);
    }

    [Fact]
    public void FlashMessage_InvalidPayload_DoesNotThrow()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        var ex = Record.Exception(() => bus.Publish(new Message("led/flash", "invalid")));

        Assert.Null(ex);
        Assert.Equal(0, led.TurnOnCount);
    }

    [Fact]
    public void FlashMessage_LedEndsInOffState()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("led/flash", "5"));

        Assert.False(led.IsOn);
    }

    [Fact]
    public void FlashMessage_AccumulatesFlashCount()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("led/flash", "2"));
        bus.Publish(new Message("led/flash", "3"));

        Assert.Equal(5, worker.FlashCount);
    }

    [Fact]
    public void FlashMessage_WrongTopic_DoesNotFlash()
    {
        var led = new FakeLedController();
        var bus = new MessageBus();
        var worker = new LedWorker(led, bus, new DeviceStatus());
        worker.Start();

        bus.Publish(new Message("wrong/topic", "5"));

        Assert.Equal(0, led.TurnOnCount);
    }
}
