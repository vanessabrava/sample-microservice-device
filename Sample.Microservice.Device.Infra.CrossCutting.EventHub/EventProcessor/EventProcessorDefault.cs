using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Sample.Microservice.Device.Infra.CrossCutting.Background;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Messages;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.EventProcessor
{
    public class EventProcessorDefault : IEventProcessor
    {
        public EventProcessorDefault(ConsumerConfigurationsOptions consumerConfiguration, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            ConsumerConfiguration = consumerConfiguration;
            ServiceProvider = serviceProvider;
            Configuration = configuration;
        }

        protected ConsumerConfigurationsOptions ConsumerConfiguration { get; }

        public string TaceKey { get; set; } = Guid.NewGuid().ToString("N");

        protected IServiceProvider ServiceProvider { get; }

        protected IConfiguration Configuration { get; }

        public virtual async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OpenAsync(PartitionContext context)
        {
            await Task.CompletedTask;
        }

        public virtual async Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            await Task.CompletedTask;
        }

        public virtual async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {

            var eventDataList = new List<EventData>();
            EventData lastEventData = null;

            if (ConsumerConfiguration.ConsumeDelayInSeconds == null)
            {
                lastEventData = messages.LastOrDefault();
                eventDataList = messages.ToList();
            }
            else
            {
                foreach (var eventData in messages)
                {
                    if (ConsumerConfiguration.ConsumeDelayInSeconds != null && eventData.SystemProperties.EnqueuedTimeUtc <= DateTime.UtcNow.AddSeconds(ConsumerConfiguration.ConsumeDelayInSeconds.Value * (-1)))
                    {
                        lastEventData = eventData;
                        eventDataList.Add(eventData);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (lastEventData != null)
            {
                try
                {
                    bool sucess = await ProcessEventDataList(context, eventDataList);

                    if (sucess)
                    {
                        await context.CheckpointAsync(lastEventData);
                    }
                    else if (!sucess && !string.IsNullOrWhiteSpace(ConsumerConfiguration.ProducerName))
                    {
                        var sendToShuntList = new List<EventData>();
                        var sendToRetryList = new List<EventData>();
                        IEventHubProducerService eventHubProducerService = ServiceProvider.GetService<IEventHubProducerService>();

                        foreach (var eventData in eventDataList)
                        {
                            if (eventData.HasIsOkProperty())
                            {
                                eventData.RemoveSendToShuntProperty().RemoveIsOkProperty().RemoveRetryProperty();
                                continue;
                            }

                            int incrementedRetryProperty = eventData.IncrementRetryProperty();
                            if (!string.IsNullOrWhiteSpace(ConsumerConfiguration.Shunt?.ProducerName)
                                && (eventData.HasSendToShuntProperty() ||
                                (ConsumerConfiguration.Shunt.MaxRetry > 0 && incrementedRetryProperty > ConsumerConfiguration.Shunt.MaxRetry))
                                )
                            {
                                eventData.RemoveSendToShuntProperty().RemoveIsOkProperty().RemoveRetryProperty();
                                sendToShuntList.Add(eventData);
                            }
                            else
                            {
                                sendToRetryList.Add(eventData);
                            }
                        }
                        if (sendToShuntList.Any()) await eventHubProducerService.SendEventDataAsync(ConsumerConfiguration.Shunt.ProducerName, sendToShuntList);
                        if (sendToRetryList.Any()) await eventHubProducerService.SendEventDataAsync(ConsumerConfiguration.ProducerName, sendToRetryList);

                        await context.CheckpointAsync(lastEventData);
                    }
                }
                catch (Exception ex)
                {

                    IEventProcessorFactory eventProcessorFactory = null;

                    try
                    {

                        await Task.Run(() =>
                        new ExecuteAsync(1, 1000, async () =>
                        {
                            IEventHubConsumerService eventHubConsumerService = ServiceProvider.GetService<IEventHubConsumerService>();

                            eventProcessorFactory = await eventHubConsumerService.UnregisterEventMessageConsumerAsync(ConsumerConfiguration.Name);

                            Thread.Sleep(10000);

                            await eventHubConsumerService.RegisterEventMessageConsumerAsync(ConsumerConfiguration.Name, eventProcessorFactory);
                        }).Do(true));

                    }
                    catch (Exception ex2)
                    {
                        if (ServiceProvider != null)
                        {
                            IEventHubConsumerService eventHubConsumerService = ServiceProvider.GetService<IEventHubConsumerService>();

                            if (eventProcessorFactory != null) await eventHubConsumerService.RegisterEventMessageConsumerAsync(ConsumerConfiguration.Name, eventProcessorFactory);
                        }
                    }
                }
            }
        }

        public virtual async Task<bool> ProcessEventDataList(PartitionContext context, List<EventData> eventDataList)
        {
            await Task.CompletedTask;
            return true;
        }

        public static string GetString(EventData eventData)
        {
            try
            {
                if (eventData?.Body == null || !eventData.Body.Array.Any() || eventData.Body.Offset < 0 || eventData.Body.Count < 0) return string.Empty;

                return Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static EventSendMessage<TEventMessage> GetEventSendMessage<TEventMessage>(EventData eventData)
        {
            var eventSendMessage = new EventSendMessage<TEventMessage>(eventData.Body.Array.FromByteArray<TEventMessage>(), new ConcurrentDictionary<string, object>(eventData.Properties));

            return eventSendMessage;
        }
        public static async Task<string> GetEventSendMessage(EventData eventData) => await Task.FromResult(eventData.Body.Array.FromByteArray<string>());
    }
}
