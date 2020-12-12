using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.EventProcessor;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Factory;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub
{
    internal class EventHubConsumerService : EventHubService, IEventHubConsumerService
    {
        private List<EventProcessorHost> EventProcessorHostList { get; }
        private Dictionary<string, IEventProcessorFactory> EventProcessorFactoryList { get; }
        private IServiceProvider ServiceProvider { get; }
        private IConfiguration Configuration { get; }

        public EventHubConsumerService(IOptionsSnapshot<EventHubOptions> options, IServiceProvider serviceProvider, IConfiguration configuration) : base(options?.Value)
        {
            ValidateOptions(options);

            ServiceProvider = serviceProvider;
            Configuration = configuration;

            EventProcessorHostList = new List<EventProcessorHost>();
            EventProcessorFactoryList = new Dictionary<string, IEventProcessorFactory>();
        }

        private EventProcessorHost eventProcessorHost;

        public async Task<bool> RegisterEventMessageConsumerAsync(string name, IEventProcessorFactory eventProcessorFactory)
        {
            ConsumerConfigurationsOptions consumerConfiguration = GetConsumerConfiguration(name);

            if (EventProcessorHostList != null && EventProcessorHostList.Any(s => s.EventHubPath == consumerConfiguration.EventHubName)) return false;

            eventProcessorHost = new EventProcessorHost(
                consumerConfiguration.EventHubName,
                !string.IsNullOrWhiteSpace(consumerConfiguration.ConsumerGroupName) ? consumerConfiguration.ConsumerGroupName : PartitionReceiver.DefaultConsumerGroupName,
                consumerConfiguration.ConnectionString,
                consumerConfiguration.StorageConnectionString,
                consumerConfiguration.StorageContainerName)
            {
                PartitionManagerOptions = new PartitionManagerOptions
                {
                    RenewInterval = TimeSpan.FromSeconds(consumerConfiguration.RenewIntervalInSeconds),
                    LeaseDuration = TimeSpan.FromSeconds(consumerConfiguration.LeaseDurationInSeconds)
                }
            };

            var eventProcessorOptions = new EventProcessorOptions()
            {
                MaxBatchSize = consumerConfiguration.NumberOfEventsPerRequest,
                ReceiveTimeout = TimeSpan.FromSeconds(consumerConfiguration.ReceiveTimeoutInSeconds)
            };

            DateTime offsetStartDateTime;
            if (!string.IsNullOrEmpty(consumerConfiguration.OffsetStartDateTime) && DateTime.TryParse(consumerConfiguration.OffsetStartDateTime, out offsetStartDateTime))
                eventProcessorOptions.InitialOffsetProvider = (partitionId) => EventPosition.FromEnqueuedTime(offsetStartDateTime);
            else
                eventProcessorOptions.InitialOffsetProvider = (partitionId) => EventPosition.FromStart();

            await eventProcessorHost.RegisterEventProcessorFactoryAsync(eventProcessorFactory, eventProcessorOptions);

            EventProcessorHostList.Add(eventProcessorHost);
            EventProcessorFactoryList.Add(name, eventProcessorFactory);

            return true;
        }

        public async Task<bool> RegisterEventMessageConsumerAsync<TEventProcessor>(string name)
            where TEventProcessor : EventProcessorDefault
        {
            ConsumerConfigurationsOptions consumerConfiguration = GetConsumerConfiguration(name);

            if (EventProcessorHostList != null && EventProcessorHostList.Any(s => s.EventHubPath == consumerConfiguration.EventHubName)) return false;

            eventProcessorHost = new EventProcessorHost(
                consumerConfiguration.EventHubName,
                !string.IsNullOrWhiteSpace(consumerConfiguration.ConsumerGroupName) ? consumerConfiguration.ConsumerGroupName : PartitionReceiver.DefaultConsumerGroupName,
                consumerConfiguration.ConnectionString,
                consumerConfiguration.StorageConnectionString,
                consumerConfiguration.StorageContainerName)
            {
                PartitionManagerOptions = new PartitionManagerOptions
                {
                    RenewInterval = TimeSpan.FromSeconds(consumerConfiguration.RenewIntervalInSeconds),
                    LeaseDuration = TimeSpan.FromSeconds(consumerConfiguration.LeaseDurationInSeconds)
                }
            };

            var eventProcessorOptions = new EventProcessorOptions()
            {
                MaxBatchSize = consumerConfiguration.NumberOfEventsPerRequest,
                ReceiveTimeout = TimeSpan.FromSeconds(consumerConfiguration.ReceiveTimeoutInSeconds)
            };

            DateTime offsetStartDateTime;
            if (!string.IsNullOrEmpty(consumerConfiguration.OffsetStartDateTime) && DateTime.TryParse(consumerConfiguration.OffsetStartDateTime, out offsetStartDateTime))
                eventProcessorOptions.InitialOffsetProvider = (partitionId) => EventPosition.FromEnqueuedTime(offsetStartDateTime);
            else
                eventProcessorOptions.InitialOffsetProvider = (partitionId) => EventPosition.FromStart();


            EventProcessorFactory<TEventProcessor> eventProcessorFactory = new EventProcessorFactory<TEventProcessor>(consumerConfiguration, ServiceProvider, Configuration);

            await eventProcessorHost.RegisterEventProcessorFactoryAsync(eventProcessorFactory, eventProcessorOptions);

            EventProcessorHostList.Add(eventProcessorHost);
            EventProcessorFactoryList.Add(name, eventProcessorFactory);

            return true;
        }

        public async Task<IEventProcessorFactory> UnregisterEventMessageConsumerAsync(string name)
        {
            ConsumerConfigurationsOptions consumerConfiguration = GetConsumerConfiguration(name);

            List<EventProcessorHost> eventProcessorHostListRegistedList = EventProcessorHostList.Where(s => s.EventHubPath == consumerConfiguration.EventHubName).ToList();

            IEventProcessorFactory returns = null;

            if (eventProcessorHostListRegistedList != null && eventProcessorHostListRegistedList.Any())
            {
                foreach (var eventProcessorHostListRegisted in eventProcessorHostListRegistedList)
                {
                    try
                    {
                        await eventProcessorHostListRegisted.UnregisterEventProcessorAsync();
                        EventProcessorHostList.Remove(eventProcessorHostListRegisted);
                        returns = EventProcessorFactoryList[name];
                        EventProcessorFactoryList.Remove(name);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return returns;
        }

        public async Task UnregisterAllMessageConsumerAsync()
        {
            foreach (var EventProcessorHost in EventProcessorHostList) await EventProcessorHost.UnregisterEventProcessorAsync();
            EventProcessorHostList.RemoveAll(s => true);
            foreach (var eventProcessorFactoryItem in EventProcessorFactoryList) EventProcessorFactoryList.Remove(eventProcessorFactoryItem.Key);
        }

        public async Task UnregisterEventProcessorAsync()
        {
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }

        private void ValidateOptions(IOptionsSnapshot<EventHubOptions> options)
        {
            if (options.Value.Consumers == null || !options.Value.Consumers.Any()) throw new ArgumentNullException("List of Consumers is null or empty.");

            var exceptionList = new List<Exception>();

            foreach (var consumer in options.Value.Consumers)
            {
                if (string.IsNullOrWhiteSpace(consumer.ConnectionString)) exceptionList.Add(new ArgumentNullException("Consumers.ConnectionString", "Value must be defined."));

                if (consumer.RenewIntervalInSeconds <= 0) exceptionList.Add(new ArgumentOutOfRangeException("Consumers.RenewIntervalInSeconds", "Value must be more than zero."));

                if (consumer.LeaseDurationInSeconds <= 0) exceptionList.Add(new ArgumentOutOfRangeException("Consumers.LeaseDurationInSeconds", "Value must be more than zero."));

                if (consumer.ConsumeDelayInSeconds != null && consumer.ConsumeDelayInSeconds.Value < 0) exceptionList.Add(new ArgumentOutOfRangeException("Consumers.ConsumeDelayInSeconds", "Value must be more than zero or null."));

                if (consumer.NumberOfEventsPerRequest <= 0) exceptionList.Add(new ArgumentOutOfRangeException("Consumers.NumberOfEventsPerRequest", "Value must be more than zero."));
            }

            if (exceptionList.Any()) throw new AggregateException("One or more errors occured.", exceptionList);
        }
    }
}