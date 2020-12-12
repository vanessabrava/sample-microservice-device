using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Options;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Helpers;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Messages;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub
{
    internal class EventHubProducerService : EventHubService, IEventHubProducerService
    {
        public EventHubProducerService(IOptionsSnapshot<EventHubOptions> options) : base(options?.Value)
        {
            ValidateOptions(options);
        }

        public async Task SendEventObjectMessageAsync<TEventMessage>(string name, EventSendMessage<TEventMessage> eventMessage)
        {
            if (eventMessage == null || eventMessage.EventMessage == null) throw new ArgumentNullException(nameof(eventMessage.EventMessage), "EventSendMessage must be defined.");

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var eventData = new EventData(eventMessage.EventMessage.ToByteArray());

            if (eventMessage.Properties != null && eventMessage.Properties.Any())
                foreach (var property in eventMessage.Properties)
                    eventData.Properties.Add(property.Key, property.Value);

            await eventHubClient.SendAsync(eventData);

            await eventHubClient.CloseAsync();
        }

        public async Task SendEventObjectMessageAsync<TEventMessage>(string name, IEnumerable<EventSendMessage<TEventMessage>> eventMessageList)
        {
            if (eventMessageList == null || !eventMessageList.Any()) throw new ArgumentNullException(nameof(eventMessageList), "Could not be null or empty.");

            var eventDataList = new List<EventData>();

            foreach (var eventMessage in eventMessageList)
            {
                if (eventMessage == null || eventMessage.EventMessage == null) throw new ArgumentNullException(nameof(eventMessage.EventMessage), "EventSendMessage must be defined.");

                var eventData = new EventData(eventMessage.EventMessage.ToByteArray());

                if (eventMessage.Properties != null && eventMessage.Properties.Any())
                    foreach (var property in eventMessage.Properties)
                        eventData.Properties.Add(property.Key, property.Value);

                eventDataList.Add(eventData);
            }

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var eventDataBatch = new EventDataBatch(long.MaxValue);

            eventDataList.ForEach(s => eventDataBatch.TryAdd(s));

            await eventHubClient.SendAsync(eventDataBatch);

            await eventHubClient.CloseAsync();
        }

        public async Task SendEventMessageAsync(string name, string eventMessage, ConcurrentDictionary<string, object> properties = null)
        {
            if (string.IsNullOrWhiteSpace(eventMessage)) throw new ArgumentNullException("eventMessage", "Message must be defined.");

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await eventHubClient.SendAsync(await GetEventData(eventMessage, properties));

            await eventHubClient.CloseAsync();
        }

        public async Task SendEventMessageAsync(string name, IEnumerable<string> eventMessageList, ConcurrentDictionary<string, object> properties = null)
        {
            if (eventMessageList == null || !eventMessageList.Any()) throw new ArgumentNullException("eventMessageList", "Could not be null or empty.");

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var eventDataBatch = new EventDataBatch(long.MaxValue);

            foreach (var eventMessage in eventMessageList)
            {
                if (string.IsNullOrWhiteSpace(eventMessage)) throw new ArgumentNullException("eventMessage", "EventSendMessage must be defined.");

                var eventData = await GetEventData(eventMessage, properties);

                eventDataBatch.TryAdd(eventData);
            }

            await eventHubClient.SendAsync(eventDataBatch);
            await eventHubClient.CloseAsync();
        }

        private static async Task<EventData> GetEventData(string eventMessage, ConcurrentDictionary<string, object> properties)
        {
            var eventData = new EventData(eventMessage.ToByteArray());

            if (properties != null && properties.Any())
                foreach (var property in properties)
                    eventData.Properties.Add(property.Key, property.Value);

            return await Task.FromResult(eventData);
        }

        private void ValidateOptions(IOptionsSnapshot<EventHubOptions> options)
        {
            if (options?.Value == null) throw new ArgumentNullException(nameof(EventHubOptions), "Must be defined or registred.");

            var exceptionList = new List<Exception>();

            foreach (var producer in options.Value.Producers)
            {
                if (string.IsNullOrWhiteSpace(producer.EventHubName)) exceptionList.Add(new ArgumentNullException("Producers.EventHubName", "Value must be defined."));

                if (string.IsNullOrWhiteSpace(producer.ConnectionString)) exceptionList.Add(new ArgumentNullException("Producers.ConnectionString", "Value must be defined."));
            }

            if (exceptionList.Any()) throw new AggregateException("One or more errors occured.", exceptionList);
        }

        public async Task SendEventDataAsync(string name, IEnumerable<EventData> eventDataList)
        {
            if (eventDataList == null || !eventDataList.Any()) throw new ArgumentNullException("eventMessageList", "Could not be null or empty.");

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var eventDataBatch = new EventDataBatch(long.MaxValue);

            foreach (var eventData in eventDataList) eventDataBatch.TryAdd(eventData);

            await eventHubClient.SendAsync(eventDataBatch);

            await eventHubClient.CloseAsync();
        }

        public async Task SendEventJsonAsync(string name, IEnumerable<string> eventJsonList, ConcurrentDictionary<string, object> properties = null)
        {
            if (eventJsonList == null || !eventJsonList.Any() || eventJsonList.Any(it => !JsonHelper.IsValidJson(it)))
                throw new ArgumentNullException("eventJsonList", "Could not be null or empty and all events must have a valid json format.");

            var eventDataList = new List<EventData>();

            foreach (var messsage in eventJsonList)
            {
                var eventData = new EventData(Encoding.UTF8.GetBytes(messsage));

                foreach (var propertie in properties)
                    eventData.Properties.Add(propertie.Key, propertie.Value);

                eventDataList.Add(eventData);
            }

            await SendEventDataAsync(name, eventDataList);
        }

        public async Task SendEventJsonAsync(string name, string eventJson, ConcurrentDictionary<string, object> properties = null)
        {
            if (string.IsNullOrWhiteSpace(eventJson) || !JsonHelper.IsValidJson(eventJson))
                throw new ArgumentNullException("eventJson", "Could not be null or empty and the event must have a valid json format.");

            ProducerConfigurationsOptions producerConfiguration = GetProducerConfiguration(name);

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(producerConfiguration.ConnectionString) { EntityPath = producerConfiguration.EventHubName };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            var eventData = new EventData(Encoding.UTF8.GetBytes(eventJson));

            if (properties != null && properties.Any())
                foreach (var propertie in properties)
                    eventData.Properties.Add(propertie.Key, propertie.Value);

            await eventHubClient.SendAsync(eventData);

            await eventHubClient.CloseAsync();
        }
    }
}