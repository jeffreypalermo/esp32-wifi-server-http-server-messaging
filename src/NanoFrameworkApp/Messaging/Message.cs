namespace NanoFrameworkApp.Messaging
{
    public class Message
    {
        public string Topic BROKEN_SYNTAX { get; }
        public string Payload { get; }

        public Message(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }
}
