using Microsoft.Azure.EventHubs;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Messages;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub
{
    public interface IEventHubProducerService
    {
        Task SendEventObjectMessageAsync<TEventMessage>(string name, EventSendMessage<TEventMessage> eventMessage);

        Task SendEventObjectMessageAsync<TEventMessage>(string name, IEnumerable<EventSendMessage<TEventMessage>> eventMessageList);

        Task SendEventMessageAsync(string name, string eventMessage, ConcurrentDictionary<string, object> properties = null);

        Task SendEventMessageAsync(string name, IEnumerable<string> eventMessageList, ConcurrentDictionary<string, object> properties = null);

        Task SendEventDataAsync(string name, IEnumerable<EventData> eventDataList);

        Task SendEventJsonAsync(string name, IEnumerable<string> eventJsonList, ConcurrentDictionary<string, object> properties = null);

        Task SendEventJsonAsync(string name, string eventJson, ConcurrentDictionary<string, object> properties = null);
    }
}
