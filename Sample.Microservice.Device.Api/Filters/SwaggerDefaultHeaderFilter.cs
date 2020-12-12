using Microsoft.OpenApi.Models;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Sample.Microservice.Device.Api.Filters
{
    internal class SwaggerDefaultHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = Headers.AuthorizationToken,
                In = ParameterLocation.Header,
                Required = true,
                Description = "Token to authorize http calls on API."
            });

            var parameters = operation.Parameters;

            operation.Parameters = parameters.Where(it => it.Name.ToLower() != "protocol" && it.Name != "DefaultRequestHeaders").ToList();
        }
    }
}
