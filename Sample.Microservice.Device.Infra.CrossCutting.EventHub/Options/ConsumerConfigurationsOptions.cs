namespace Sample.Microservice.Device.Infra.CrossCutting.EventHub.Options
{
    public class ConsumerConfigurationsOptions
    {
        public string Name { get; set; }

        public string ProducerName { get; set; }

        public ShuntOptions Shunt { get; set; }

        public string ConnectionString { get; set; }

        public string EventHubName { get; set; }

        public int? ConsumeDelayInSeconds { get; set; }

        public int NumberOfEventsPerRequest { get; set; } = 1;

        public int RenewIntervalInSeconds { get; set; } = 10;

        public int LeaseDurationInSeconds { get; set; } = 60;

        public int ReceiveTimeoutInSeconds { get; set; } = 60;

        public string OffsetStartDateTime { get; set; } = "";

        public string ConsumerGroupName { get; set; }

        public string StorageContainerName { get; set; }

        public string StorageAccountName { get; set; }

        public string StorageAccountKey { get; set; }

        internal string StorageConnectionString => $"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey}";
    }
}