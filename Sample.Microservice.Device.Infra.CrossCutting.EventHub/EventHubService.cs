using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub
{
    internal abstract class EventHubService
    {
        protected EventHubService(EventHubOptions eventHubOptions) => EventHubOptions = eventHubOptions;

        protected EventHubOptions EventHubOptions { get; }

        protected ConsumerConfigurationsOptions GetConsumerConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name", "Could not be null or empty.");

            ConsumerConfigurationsOptions consumerConfiguration = EventHubOptions.Consumers?.FirstOrDefault(s => s.Name == name);

            if (consumerConfiguration == null) throw new KeyNotFoundException($"Configuration section named as {name} at 'EventHubs':Consumers was not found.");

            return consumerConfiguration;
        }

        protected ProducerConfigurationsOptions GetProducerConfiguration(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name", "Could not be null or empty.");

            ProducerConfigurationsOptions producerConfiguration = EventHubOptions.Producers?.FirstOrDefault(s => s.Name == name);

            if (producerConfiguration == null) throw new KeyNotFoundException($"Configuration section named as {name} at 'EventHubs':Consumers was not found.");

            return producerConfiguration;
        }
    }
}
