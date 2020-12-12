using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sample.Microservice.Device.Api.Configurations;
using HostBuilder = Microsoft.Extensions.Hosting.Host;

namespace Sample.Microservice.Device.Api
{
    /// <summary>
    /// Program class to host configure.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected Program() { }

        /// <summary>
        /// Main method to start configuration.
        /// </summary>
        /// <param name="args">The arguments of configuration.</param>
        public static void Main(string[] args) => CreateHostBuilder(args).Run();


        /// <summary>
        /// The method to create and build the host.
        /// </summary>
        /// <param name="args">The arguments of configuration.</param>
        /// <returns></returns>
        public static IHost CreateHostBuilder(string[] args) =>
            HostBuilder
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = null;
                    options.Limits.MinResponseDataRate = null;
                })
                .ConfigureAppConfiguration((hostingContext, configuration) => hostingContext.ConfigAppSettingsFiles(configuration))
                .UseStartup<Startup>();
            })
            .Build();
    }
}
