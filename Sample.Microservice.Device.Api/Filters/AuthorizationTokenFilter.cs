using Microsoft.AspNetCore.Mvc.Filters;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Api.Filters
{
    internal class AuthorizationTokenFilter : IAsyncActionFilter
    {
        public string AuthorizationToken { get; }

        public AuthorizationTokenFilter(string authorizationToken) => AuthorizationToken = authorizationToken;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authorizationToken = context.HttpContext.Request.Headers[Headers.AuthorizationToken].ToString();

            if (string.IsNullOrEmpty(authorizationToken) || authorizationToken != AuthorizationToken)
            {
                var requestMessage = new RequestMessage();

                var protocol = context.HttpContext.Request.Headers[Headers.Protocol].ToString();

                if (string.IsNullOrWhiteSpace(protocol) || !Guid.TryParse(protocol, out Guid ProtocolParsed))
                {
                    protocol = Guid.NewGuid().ToString("N");

                    if (string.IsNullOrWhiteSpace(protocol))
                        context.HttpContext.Request.Headers.Add(Headers.Protocol, protocol);
                    else
                        context.HttpContext.Request.Headers[Headers.Protocol] = protocol;
                }

                requestMessage.AddHeader(Headers.Protocol, protocol);

                var result = new ResultResponseMessage<ResponseMessage>(requestMessage);
                result.CreateResponseUnauthorized();

                context.Result = result;
                return;
            }

            await next();
        }
    }
}