using System.Collections.Concurrent;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Messages
{
    public class EventSendMessage<TEventMessage>
    {
        public EventSendMessage() { }

        public EventSendMessage(TEventMessage eventMessage, ConcurrentDictionary<string, object> properties)
        {
            EventMessage = eventMessage;
            Properties = properties;
        }

        public ConcurrentDictionary<string, object> Properties { get; set; }

        public TEventMessage EventMessage { get; set; }
    }
}