using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Api.Filters
{
    internal class RequestMessageValidationFilterAttribute : ActionFilterAttribute
    {
        private RequestFilterConfigure Configure { get; }
        
        public RequestMessageValidationFilterAttribute() => Configure = new RequestFilterConfigure();

        public RequestMessageValidationFilterAttribute(IEnumerable<string> headerKeysToExtract)
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
            foreach (var kvp in context.ActionArguments)
                if (kvp.Value is RequestMessage requestMessage)
                    ExecuteValidationInArgument(context, requestMessage);
        }

        private void ExecuteValidationInArgument<TRequestMessage>(ActionExecutingContext context, TRequestMessage requestMessage)
            where TRequestMessage : RequestMessage
        {
            Configure.SetProtocolToRequest(context, requestMessage);
            Configure.SetOriginIp(context);
            Configure.SetOriginDevice(context);
            Configure.SetCustomHeaders(context, requestMessage);

            var result = new ResultResponseMessage<ResponseMessage>(requestMessage);

            if (requestMessage == null)
            {
                result.CreateResponseBadRequest("The request cannot be null.");
                context.Result = result;
                return;
            }

            if (context.ModelState.IsValid)
                return;

            foreach (KeyValuePair<string, ModelStateEntry> kvp in context.ModelState)
            {
                string key = kvp.Key.Split('.').LastOrDefault();
                ModelStateEntry value = kvp.Value;

                if (value.Errors == null || !value.Errors.Any())
                    continue;

                var errorMessage = value.Errors.First().ErrorMessage;

                key = char.ToLowerInvariant(key[0]) + key.Substring(1);

                if (string.IsNullOrWhiteSpace(errorMessage) || errorMessage.ToLower().Contains("field is required"))
                    result.AddValidation(key, "The field " + key + " is required");
                else
                    result.AddValidation(key, value.Errors.FirstOrDefault()?.ErrorMessage);
            }

            result.CreateResponseBadRequest("Fields Validation");
            context.Result = result;
        }
    }
}