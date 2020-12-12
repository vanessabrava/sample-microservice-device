using Sample.Microservice.Device.Infra.DataAccess.Repositories;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.DataAccess.MySql.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        public async Task NewAsync(Service.Model.Device model) => await Task.CompletedTask;
    }
}
