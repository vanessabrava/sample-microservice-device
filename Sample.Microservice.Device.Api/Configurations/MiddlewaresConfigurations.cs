using Microsoft.AspNetCore.Builder;

namespace Sample.Microservice.Device.Api.Configurations
{
    internal static class MiddlewaresConfigurations
    {
        public static IApplicationBuilder UseMiddlewares<TStartup>(this IApplicationBuilder applicationBuilder)
           where TStartup : class
        {
            applicationBuilder.UseMiddleware<ErrorHandlerMiddleware<TStartup>>();
            return applicationBuilder;
        }

        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder applicationBuilder, string swaggerDocTitle, string swaggerDocVersion)
        {
            applicationBuilder.UseSwagger();
            applicationBuilder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerDocTitle + " " + swaggerDocVersion.ToUpper());
            });

            return applicationBuilder;
        }
    }
}
