using System.Collections;

namespace NanoFrameworkApp.Messaging
{
    public delegate void MessageHandler(Message message);

    public class MessageBus
    {
        private readonly Hashtable _subscriptions = new Hashtable();
        private readonly object _syncLock = new object();

        public void Subscribe(string topic, MessageHandler handler)
        {
            lock (_syncLock)
            {
                if (!_subscriptions.Contains(topic))
                {
                    _subscriptions[topic] = new ArrayList();
                }

                ((ArrayList)_subscriptions[topic]).Add(handler);
            }
        }

        public void Publish(Message message)
        {
            ArrayList handlers;

            lock (_syncLock)
            {
                if (!_subscriptions.Contains(message.Topic))
                {
                    return;
                }

                // Copy the handler list so we invoke outside the lock
                ArrayList original = (ArrayList)_subscriptions[message.Topic];
                handlers = new ArrayList();
                for (int i = 0; i < original.Count; i++)
                {
                    handlers.Add(original[i]);
                }
            }

            for (int i = 0; i < handlers.Count; i++)
            {
                ((MessageHandler)handlers[i])(message);
            }
        }

        public int SubscriberCount(string topic)
        {
            lock (_syncLock)
            {
                if (!_subscriptions.Contains(topic))
                {
                    return 0;
                }

                return ((ArrayList)_subscriptions[topic]).Count;
            }
        }
    }
}
