using nanoFramework.TestFramework;
using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.Tests
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void TestMessageCreation()
        {
            string topic = "test/topic";
            string payload = "hello";

            Message message = new Message(topic, payload);

            Assert.AreEqual(topic, message.Topic, "Topic should match the value passed to constructor");
            Assert.AreEqual(payload, message.Payload, "Payload should match the value passed to constructor");
        }

        [TestMethod]
        public void TestMessageWithEmptyPayload()
        {
            Message message = new Message("test/topic", "");

            Assert.AreEqual("test/topic", message.Topic);
            Assert.AreEqual("", message.Payload, "Empty string payload should be preserved");
        }

        [TestMethod]
        public void TestMessageWithNullPayload()
        {
            Message message = new Message("test/topic", null);

            Assert.AreEqual("test/topic", message.Topic);
            Assert.IsNull(message.Payload, "Null payload should be preserved");
        }
    }
}
