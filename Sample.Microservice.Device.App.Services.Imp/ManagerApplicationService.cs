using Microsoft.Azure.EventHubs;
using Sample.Microservice.Device.App.Mapper;
using Sample.Microservice.Device.App.Messages;
using Sample.Microservice.Device.App.Services.Imp.Extensions;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using Sample.Microservice.Device.Service;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.App.Services.Imp
{
    public class ManagerApplicationService : IManagerApplicationService
    {
        private string EventHubName => "SampleDevice";

        private IEventHubProducerService EventHubProducerServices { get; }

        private IManagerService ManagerService { get; }

        public ManagerApplicationService(IManagerService managerService, IEventHubProducerService eventHubProducerServices)
        {
            EventHubProducerServices = eventHubProducerServices;
            ManagerService = managerService;
        }

        public async Task<ResultResponseMessage> NewAsync(DeviceRequestMessage request)
        {
            var modelResult = DeviceMapper.FromMessage(request?.DeviceMessage);

            if (!modelResult.IsModelResultValid())
                return await Task.FromResult(modelResult.ToResultResponseMessage(request));

            modelResult = await ManagerService.NewAsync(modelResult.Model);

            if (!modelResult.IsModelResultValid())
                return await Task.FromResult(modelResult.ToResultResponseMessage(request));

            await SendEvent(request.DeviceMessage, request.GetHeader(Headers.Protocol));

            var result = modelResult.ToResultResponseMessage(request);

            result.CreateResponseNoContent();

            return await Task.FromResult(result);
        }


        private async Task SendEvent(DeviceMessage deviceMessage, string protocol)
        {
            var headers = new ConcurrentDictionary<string, object>(new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(Headers.Protocol, protocol)
            });

            await EventHubProducerServices.SendEventDataAsync(EventHubName, GetEventData(JsonSerializer.Serialize(deviceMessage), headers));
            await Task.CompletedTask;

        }

        private IEnumerable<EventData> GetEventData(string message, ConcurrentDictionary<string, object> headers)
        {
            var eventDataList = new List<EventData>();

            var eventData = new EventData(Encoding.UTF8.GetBytes(message));

            foreach (var header in headers)
                eventData.Properties.Add(header.Key, header.Value);

            eventDataList.Add(eventData);

            return eventDataList;
        }
    }
}
