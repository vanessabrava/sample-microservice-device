using Microsoft.AspNetCore.Mvc.Filters;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class RequestMessageFilterAttribute : ActionFilterAttribute
    {
        private RequestFilterConfigure Configure { get; }

        public RequestMessageFilterAttribute() => Configure = new RequestFilterConfigure();

        public RequestMessageFilterAttribute(IEnumerable<string> headerKeysToExtract)
            : this()
        {
            var newHeaderKeysToExtract = new List<string> { Headers.OriginIp, Headers.OriginDevice };
            if (headerKeysToExtract != null && headerKeysToExtract.Any())
            {
                foreach (var headerKeyToExtract in headerKeysToExtract)
                    if (!Configure.HeaderKeysToExtract.Any(it => it == headerKeyToExtract))
                        newHeaderKeysToExtract.Add(headerKeyToExtract);
            }

            Configure.HeaderKeysToExtract = newHeaderKeysToExtract;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var keys = context.ActionArguments.Keys.ToList();

            foreach (string key in keys)
            {
                object paramValue = context.ActionArguments[key];

                var paramDescriptor = context.ActionDescriptor.Parameters.FirstOrDefault(it => it.Name == key);

                if (paramDescriptor == null)
                    continue;

                if (!paramDescriptor.ParameterType.IsSubclassOf(typeof(RequestMessage)) || !paramDescriptor.ParameterType.IsEquivalentTo(typeof(RequestMessage)))
                    continue;

                if (paramValue == null)
                {
                    paramValue = Activator.CreateInstance(paramDescriptor.ParameterType);
                    context.ActionArguments[key] = paramValue;
                }

                if (!(paramValue is RequestMessage requestMessage))
                    requestMessage = default;

                if (requestMessage != null)
                {
                    Configure.SetProtocolToRequest(context, requestMessage);
                    Configure.SetOriginIp(context);
                    Configure.SetOriginDevice(context);
                    Configure.SetCustomHeaders(context, requestMessage);
                }
            }
        }
    }
}