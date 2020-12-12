using Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Service
{
    public interface IManagerService
    {
        Task<IModelResult<Model.Device>> NewAsync(Model.Device model);
    }
}
