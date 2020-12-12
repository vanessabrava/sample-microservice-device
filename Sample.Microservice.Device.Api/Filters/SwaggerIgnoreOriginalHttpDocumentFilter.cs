using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Sample.Microservice.Device.Api.Filters
{
    /// <summary>
    /// Filter responsible for ignore the original http documents on swagger.
    /// </summary>
    internal class SwaggerIgnoreOriginalHttpDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Apply operation on filter.
        /// </summary>
        /// <param name="swaggerDoc">Document that including filters.</param>
        /// <param name="context">Context of document filter.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var item in swaggerDoc.Paths)
                GetOriginalHttpParametersForMethod(item.Value?.Parameters);
        }

        private void GetOriginalHttpParametersForMethod(IList<OpenApiParameter> parametersToGet)
        {
            var parametersToRemove = new List<OpenApiParameter>();

            if (parametersToGet != null)
            {
                foreach (var itemGet in parametersToGet)
                    if (itemGet.Name.Contains("OriginalHttp"))
                        parametersToRemove.Add(itemGet);

                foreach (var param in parametersToRemove)
                    parametersToGet.Remove(param);
            }
        }
    }
}
