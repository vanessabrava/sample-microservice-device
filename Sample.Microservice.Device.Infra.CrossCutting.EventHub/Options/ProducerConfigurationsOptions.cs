namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options
{
    public class ProducerConfigurationsOptions
    {
        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string EventHubName { get; set; }
    }
}
