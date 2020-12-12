using Microsoft.Extensions.DependencyInjection;
using Sample.Microservice.Device.App.Services;
using Sample.Microservice.Device.App.Services.Imp;
using Sample.Microservice.Device.Infra.DataAccess.MySql.Repositories;
using Sample.Microservice.Device.Infra.DataAccess.Repositories;
using Sample.Microservice.Device.Service;
using Sample.Microservice.Device.Service.Imp;

namespace Sample.Microservice.Device.Infra.CrossCutting.DI
{
    public static class DIFactory
    {
        public static void ConfigureDI(IServiceCollection services)
        {
            services.AddScoped<IManagerApplicationService, ManagerApplicationService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
        }
    }
}
