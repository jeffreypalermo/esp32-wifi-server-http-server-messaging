using System.Threading;
using nanoFramework.TestFramework;
using NanoFrameworkApp.Messaging;
using NanoFrameworkApp.Tests.Hardware;
using NanoFrameworkApp.Workers;

namespace NanoFrameworkApp.Tests
{
    [TestClass]
    public class LedWorkerTests
    {
        [TestMethod]
        public void TestLedWorkerStartSetsRunning()
        {
            MessageBus bus = new MessageBus();
            FakeLedController led = new FakeLedController();
            LedWorker worker = new LedWorker(led, bus);

            worker.Start();

            Assert.IsTrue(worker.IsRunning, "Worker should be running after Start()");

            worker.Stop();
        }

        [TestMethod]
        public void TestLedWorkerName()
        {
            MessageBus bus = new MessageBus();
            FakeLedController led = new FakeLedController();
            LedWorker worker = new LedWorker(led, bus);

            Assert.AreEqual("LedWorker", worker.Name, "Worker name should be LedWorker");
        }

        [TestMethod]
        public void TestLedFlashOnMessage()
        {
            MessageBus bus = new MessageBus();
            FakeLedController led = new FakeLedController();
            LedWorker worker = new LedWorker(led, bus);

            worker.Start();

            bus.Publish(new Message("led/flash", "2"));

            // Allow time for the LED to flash
            Thread.Sleep(3000);

            Assert.IsTrue(led.TurnOnCount >= 2,
                "TurnOnCount should be at least 2 but was " + led.TurnOnCount.ToString());

            worker.Stop();
        }

        [TestMethod]
        public void TestLedFlashWithPayloadOne()
        {
            MessageBus bus = new MessageBus();
            FakeLedController led = new FakeLedController();
            LedWorker worker = new LedWorker(led, bus);

            worker.Start();

            bus.Publish(new Message("led/flash", "1"));

            // Allow time for a single flash
            Thread.Sleep(2000);

            Assert.IsTrue(led.TurnOnCount >= 1,
                "TurnOnCount should be at least 1 but was " + led.TurnOnCount.ToString());
            Assert.IsTrue(led.TurnOffCount >= 1,
                "TurnOffCount should be at least 1 but was " + led.TurnOffCount.ToString());

            worker.Stop();
        }

        [TestMethod]
        public void TestLedWorkerStop()
        {
            MessageBus bus = new MessageBus();
            FakeLedController led = new FakeLedController();
            LedWorker worker = new LedWorker(led, bus);

            worker.Start();
            Assert.IsTrue(worker.IsRunning, "Worker should be running after Start()");

            worker.Stop();
            Assert.IsFalse(worker.IsRunning, "Worker should not be running after Stop()");
        }
    }
}
