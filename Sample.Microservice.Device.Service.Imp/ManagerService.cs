using Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules;
using Sample.Microservice.Device.Infra.DataAccess.Repositories;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Service.Imp
{
    public class ManagerService : IManagerService
    {
        private IDeviceRepository DeviceRepository { get; }

        public ManagerService(IDeviceRepository deviceRepository) => DeviceRepository = deviceRepository;

        public async Task<IModelResult<Model.Device>> NewAsync(Model.Device model)
        {
            var modelResult = new ModelResult<Model.Device>();

            if (model == null)
                return await Task.FromResult(modelResult);

            await DeviceRepository.NewAsync(model);

            modelResult.SetModel(model);

            return await Task.FromResult(modelResult);


        }
    }
}
