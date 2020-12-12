using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Microservice.Device.Infra.CrossCutting.DI;
using Sample.Microservice.Device.Infra.CrossCutting.EventHub.Extensions;

namespace Sample.Microservice.Device.Api.Configurations
{
    internal static class DependencyInjectionConfigurations
    {
        public static IServiceCollection ConfigureDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEventHub(configuration);

            DIFactory.ConfigureDI(services);

            return services;
        }
    }
}
