using Sample.Microservice.Device.App.Mapper;
using Sample.Microservice.Device.App.Messages;
using Sample.Microservice.Device.App.Services.Imp.Extensions;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using Sample.Microservice.Device.Service;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.App.Services.Imp
{
    public class ManagerApplicationService : IManagerApplicationService
    {
        private IManagerService ManagerService { get; }

        public ManagerApplicationService(IManagerService managerService) => ManagerService = managerService;

        public async Task<ResultResponseMessage> NewAsync(DeviceRequestMessage request)
        {
            var modelResult = DeviceMapper.FromMessage(request?.DeviceMessage);

            if (!modelResult.IsModelResultValid())
                return await Task.FromResult(modelResult.ToResultResponseMessage(request));

            modelResult = await ManagerService.NewAsync(modelResult.Model);

            if (!modelResult.IsModelResultValid())
                return await Task.FromResult(modelResult.ToResultResponseMessage(request));

            var result = modelResult.ToResultResponseMessage(request);

            result.CreateResponseNoContent();

            return await Task.FromResult(result);
        }
    }
}
