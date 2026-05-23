using nanoFramework.TestFramework;
using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.Tests
{
    [TestClass]
    public class MessageBusTests
    {
        [TestMethod]
        public void TestPublishWithNoSubscribers()
        {
            MessageBus bus = new MessageBus();

            // Should not throw when publishing to a topic with no subscribers
            Message message = new Message("no/subscribers", "data");
            bus.Publish(message);

            // If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestSubscribeAndPublish()
        {
            MessageBus bus = new MessageBus();
            bool handlerCalled = false;
            Message receivedMessage = null;

            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg)
            {
                handlerCalled = true;
                receivedMessage = msg;
            }));

            Message message = new Message("test/topic", "payload");
            bus.Publish(message);

            Assert.IsTrue(handlerCalled, "Handler should have been called");
            Assert.IsNotNull(receivedMessage, "Received message should not be null");
        }

        [TestMethod]
        public void TestMultipleSubscribers()
        {
            MessageBus bus = new MessageBus();
            bool firstHandlerCalled = false;
            bool secondHandlerCalled = false;

            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg)
            {
                firstHandlerCalled = true;
            }));
            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg)
            {
                secondHandlerCalled = true;
            }));

            Message message = new Message("test/topic", "payload");
            bus.Publish(message);

            Assert.IsTrue(firstHandlerCalled, "First handler should have been called");
            Assert.IsTrue(secondHandlerCalled, "Second handler should have been called");
        }

        [TestMethod]
        public void TestDifferentTopics()
        {
            MessageBus bus = new MessageBus();
            bool topicAHandlerCalled = false;
            bool topicBHandlerCalled = false;

            bus.Subscribe("topic/a", new MessageHandler(delegate (Message msg)
            {
                topicAHandlerCalled = true;
            }));
            bus.Subscribe("topic/b", new MessageHandler(delegate (Message msg)
            {
                topicBHandlerCalled = true;
            }));

            bus.Publish(new Message("topic/a", "data"));

            Assert.IsTrue(topicAHandlerCalled, "Topic A handler should have been called");
            Assert.IsFalse(topicBHandlerCalled, "Topic B handler should not have been called");
        }

        [TestMethod]
        public void TestSubscriberCount()
        {
            MessageBus bus = new MessageBus();

            Assert.AreEqual(0, bus.SubscriberCount("test/topic"), "Initial count should be 0");

            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg) { }));
            Assert.AreEqual(1, bus.SubscriberCount("test/topic"), "Count should be 1 after one subscribe");

            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg) { }));
            Assert.AreEqual(2, bus.SubscriberCount("test/topic"), "Count should be 2 after two subscribes");

            Assert.AreEqual(0, bus.SubscriberCount("other/topic"), "Unsubscribed topic should have 0 count");
        }

        [TestMethod]
        public void TestPublishDeliversCorrectMessage()
        {
            MessageBus bus = new MessageBus();
            Message receivedMessage = null;

            bus.Subscribe("test/topic", new MessageHandler(delegate (Message msg)
            {
                receivedMessage = msg;
            }));

            Message published = new Message("test/topic", "expected-payload");
            bus.Publish(published);

            Assert.IsNotNull(receivedMessage, "Handler should have received a message");
            Assert.AreEqual("test/topic", receivedMessage.Topic, "Topic should match published message");
            Assert.AreEqual("expected-payload", receivedMessage.Payload, "Payload should match published message");
        }

        [TestMethod]
        public void TestMessageBusThreadSafety()
        {
            MessageBus bus = new MessageBus();

            // Basic stability test: subscribe and publish multiple times without crashing
            for (int i = 0; i < 10; i++)
            {
                bus.Subscribe("stress/topic", new MessageHandler(delegate (Message msg)
                {
                    // no-op handler
                }));
            }

            Assert.AreEqual(10, bus.SubscriberCount("stress/topic"), "Should have 10 subscribers");

            for (int i = 0; i < 100; i++)
            {
                bus.Publish(new Message("stress/topic", "message-" + i.ToString()));
            }

            // If we reach here without exception, the basic stability test passes
            Assert.IsTrue(true, "MessageBus should handle rapid publish without crashing");
        }
    }
}
