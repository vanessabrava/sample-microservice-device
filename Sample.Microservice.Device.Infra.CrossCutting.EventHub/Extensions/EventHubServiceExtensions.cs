using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions
{
    public static class EventHubServiceExtensions
    {
        public static IServiceCollection AddEventHub(this IServiceCollection services, IConfiguration configuration, string appSettingsEventHubSection = "EventHubs")
        {
            var eventHubOptions = new EventHubOptions();
            configuration.GetSection(appSettingsEventHubSection).Bind(eventHubOptions);

            if (eventHubOptions == null) throw new ArgumentException("EventHub configuration mus be defined.", "appSettingsEventHubSection");

            Dictionary<string, int> consumersName = eventHubOptions.Consumers?.GroupBy(s => s.Name).ToDictionary(s => s.Key, s => s.Count());
            Dictionary<string, int> producersName = eventHubOptions.Producers?.GroupBy(s => s.Name).ToDictionary(s => s.Key, s => s.Count());

            if (consumersName != null && consumersName.Any(s => s.Value > 1) || producersName != null && producersName.Any(s => s.Value > 1))
                throw new NotSupportedException($"EventHub name(s) {string.Join(", ", consumersName.Where(s => s.Value > 1).Select(s => s.Key).Union(producersName.Where(s => s.Value > 1).Select(s => s.Key)).Distinct())} was repeated at same location.");

            if (consumersName != null && producersName != null && consumersName.Any(s => producersName.Any(d => d.Key == s.Key)))
                throw new NotSupportedException($"EventHub name(s) {string.Join(", ", consumersName.Where(s => producersName.Any(d => d.Key == s.Key)).Select(s => s.Key))} was repeated with differents locations.");

            services.AddOptions();
            services.Configure<EventHubOptions>(options => configuration.GetSection(appSettingsEventHubSection).Bind(options));
            services.AddScoped<IEventHubProducerService, EventHubProducerService>();
            services.AddScoped<IEventHubConsumerService, EventHubConsumerService>();

            return services;
        }
    }
}
