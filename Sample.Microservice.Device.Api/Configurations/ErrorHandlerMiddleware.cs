using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Sample.Microservice.Device.Infra.CrossCutting.Options;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Api.Configurations
{
    internal class ErrorHandlerMiddleware<TStartup>
        where TStartup : class
    {
        private RequestDelegate NextMiddlewareInPipelineDelegate { get; }
        private bool ViewDetailsOnResponse { get; }
        public ErrorHandlerMiddleware(RequestDelegate nextMiddlewareInPipelineDelegate, IOptions<ErrorLogOptions> errorLogOptions)
        {
            NextMiddlewareInPipelineDelegate = nextMiddlewareInPipelineDelegate;

            if (errorLogOptions?.Value == null)
                throw new ArgumentException("You must enter log settings in ErrorLogOptions.");

            ViewDetailsOnResponse = errorLogOptions.Value.ViewDetailsOnResponse;
        }

        public async Task Invoke(HttpContext context)
        {
            try { await NextMiddlewareInPipelineDelegate(context); } catch (Exception ex) { await HandleExceptionAsync(context, ex); }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (ex is AggregateException)
                ex = ex.InnerException;

            var errorMessage = ViewDetailsOnResponse ? ex.ToString() : Messages.ErrorDefault;

            var protocol = context.Request.Headers[Headers.Protocol].ToString();

            if (string.IsNullOrWhiteSpace(protocol) || !Guid.TryParse(protocol, out Guid protocolParsed))
            {
                protocol = Guid.NewGuid().ToString("N");

                if (string.IsNullOrWhiteSpace(protocol))
                    context.Request.Headers.Add(Headers.Protocol, protocol);
                else
                    context.Request.Headers[Headers.Protocol] = protocol;
            }

            var requestMessage = new RequestMessage();
            requestMessage.AddHeader(Headers.Protocol, protocol);

            var result = new ResultResponseMessage(requestMessage);

            result.CreateResponseInternalServerError(errorMessage);

            var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)result.HttpStatusCode;
            await context.Response.WriteAsync(jsonResult);
        }
    }
}