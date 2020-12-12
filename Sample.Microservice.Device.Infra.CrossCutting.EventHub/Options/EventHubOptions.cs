using System.Collections.Generic;

namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options
{
    public class EventHubOptions
    {
        public List<ProducerConfigurationsOptions> Producers { get; set; }

        public List<ConsumerConfigurationsOptions> Consumers { get; set; }
    }
}
