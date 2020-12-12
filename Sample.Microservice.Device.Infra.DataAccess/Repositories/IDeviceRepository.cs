using System.Threading.Tasks;
using Model = Sample.Microservice.Device.Service.Model;

namespace Sample.Microservice.Device.Infra.DataAccess.Repositories
{
    public interface IDeviceRepository
    {
        Task NewAsync(Model.Device model);
    }
}
