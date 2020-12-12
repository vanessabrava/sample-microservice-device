using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.EventProcessor;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Factory
{
    internal class EventProcessorFactory<TEventProcessorDefault> : IEventProcessorFactory where TEventProcessorDefault : EventProcessorDefault
    {
        private ConsumerConfigurationsOptions ConsumerConfiguration { get; }
        private int? ConsumeDelayInSeconds => ConsumerConfiguration.ConsumeDelayInSeconds;
        private IServiceProvider Services { get; }
        private IConfiguration Configuration { get; }

        public EventProcessorFactory(ConsumerConfigurationsOptions consumerConfiguration, IServiceProvider services, IConfiguration configuration)
        {
            var exceptionList = new List<Exception>();

            if (services == null) exceptionList.Add(new ArgumentNullException("services", "Could not be null."));
            if (configuration == null) exceptionList.Add(new ArgumentNullException("configuration", "Could not be null."));

            if (exceptionList.Any()) throw new AggregateException("One or more errors occured.", exceptionList);

            var constructors = typeof(TEventProcessorDefault).GetConstructors();

            if (constructors.Count() != 1) throw new ArgumentOutOfRangeException("'TEventProcessor' should has one constructor.");

            var parameters = constructors.First().GetParameters();

            if (parameters.Count() != 3) throw new ArgumentOutOfRangeException("'TEventProcessor' constructor should one constructor that containing the 3 parameters of EventProcessorDefault class.");

            try
            {
                var checkInstance = (IEventProcessor)(Activator.CreateInstance(typeof(TEventProcessorDefault), ConsumerConfiguration, services, configuration));
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException($"{typeof(TEventProcessorDefault).FullName} constructor is invalid. Its should has only three parameters: System.Int32?, System.IServiceProvider, Microsoft.Extensions.Configuration.IConfiguration", ex);
            }

            Services = services;
            Configuration = configuration;
            ConsumerConfiguration = consumerConfiguration;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            IEventProcessor eventProcessor = (IEventProcessor)(Activator.CreateInstance(typeof(TEventProcessorDefault), ConsumerConfiguration, Services, Configuration));

            return eventProcessor;
        }
    }
}