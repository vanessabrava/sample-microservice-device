using Microsoft.AspNetCore.Mvc;
using Sample.Microservice.Device.App.Messages;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.App.Services
{
    public interface IManagerApplicationService
    {
        Task<ResultResponseMessage> NewAsync(DeviceRequestMessage request);
    }
}
