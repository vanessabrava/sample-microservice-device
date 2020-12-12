using Microsoft.AspNetCore.Mvc;
using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Infra.CrossCutting.Services
{
    [Serializable]
    public class ResultResponseMessage : IActionResult
    {
        public ResultResponseMessage() => CreateResponseOk();

        public ResultResponseMessage(RequestMessage requestMessage) : this() => SetRequestMessage(requestMessage);

        [JsonPropertyName("message")]
        public string Message { get; private set; }

        [JsonPropertyName("validations")]
        public IEnumerable<ValidationMessage> Validations { get; private set; }

        [JsonPropertyName("protocol")]
        public string Protocol { get; private set; }

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; private set; }

        public void SetRequestMessage(RequestMessage requestMessage)
        {
            if (requestMessage == null)
                throw new ArgumentNullException("The request cannot be null.");

            if (!Guid.TryParse(requestMessage.GetHeader(Headers.Protocol), out Guid protocol))
                throw new ArgumentException("Invalid Protocol in the request.");

            Protocol = protocol.ToString("N");
        }

        public void MapResultResponseMessage(HttpStatusCode httpStatusCode, string message = null, string protocol = null, IEnumerable<ValidationMessage> validations = null)
        {
            if (!string.IsNullOrWhiteSpace(protocol))
                Protocol = protocol;

            if (validations != null)
                Validations = validations;

            CreateResponseToHttpStatusCode(httpStatusCode, message);
        }

        public void AddValidation(string attribute, string message)
        {
            var validations = new List<ValidationMessage>();

            if (Validations != null && Validations.Any())
                foreach (var validation in Validations)
                    validations.Add(validation);

            validations.Add(new ValidationMessage { Attribute = attribute, Message = message });

            Validations = validations;
        }

        public void AddValidation(IEnumerable<ValidationMessage> validations)
        {
            var consolidateValidations = new List<ValidationMessage>();

            if (Validations != null && Validations.Any())
                foreach (var validation in Validations)
                    consolidateValidations.Add(validation);

            consolidateValidations.AddRange(validations);

            Validations = consolidateValidations;
        }

        public bool IsHttpStatusCodeError()
        {
            if (Validations != null && Validations.Any() && (int)HttpStatusCode < 400)
                CreateResponseBadRequest();

            return (int)HttpStatusCode >= 400;
        }

        public void CreateResponseOk()
        {
            if (Validations == null || !Validations.Any())
            {
                Message = "Success.";
                HttpStatusCode = (HttpStatusCode)200;
            }
        }

        public void CreateResponseCreated()
        {
            Message = "Created successfully.";
            HttpStatusCode = (HttpStatusCode)201;
        }

        public void CreateResponseAccepted()
        {
            Message = "Processing in background.";
            HttpStatusCode = (HttpStatusCode)202;
        }

        public void CreateResponseNoContent()
        {
            Message = "No content on result.";
            HttpStatusCode = (HttpStatusCode)204;
        }

        public void CreateResponseBadRequest(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Validation." : message;
            HttpStatusCode = (HttpStatusCode)400;
        }

        public void CreateResponseUnauthorized(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Unauthorized access to the resource." : message;
            HttpStatusCode = (HttpStatusCode)401;
        }

        public void CreateResponseForbidden()
        {
            Message = "The server refused the request.";
            HttpStatusCode = (HttpStatusCode)403;
        }

        public void CreateResponseNotFound()
        {
            Message = "Route or resource not found on server.";
            HttpStatusCode = (HttpStatusCode)404;
        }

        public void CreateResponseNotAcceptable()
        {
            Message = "Header information is required.";
            HttpStatusCode = (HttpStatusCode)406;
        }

        public void CreateResponseRequestTimeout(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Timeout reached on request." : message;
            HttpStatusCode = HttpStatusCode.RequestTimeout;
        }

        public void CreateResponseUnprocessableEntity(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Validation." : message;
            HttpStatusCode = (HttpStatusCode)422;
        }

        public void CreateResponseInternalServerError(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? Messages.ErrorDefault : message;
            HttpStatusCode = (HttpStatusCode)500;
        }

        public void CreateResponseServiceUnavailable(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Service Unavailable." : message;
            HttpStatusCode = (HttpStatusCode)503;
        }

        public void CreateResponseGatewayTimeout(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "Timeout reached at gateway." : message;
            HttpStatusCode = HttpStatusCode.GatewayTimeout;
        }

        private void CreateResponseToHttpStatusCode(HttpStatusCode httpStatusCode, string message)
        {
            switch ((int)httpStatusCode)
            {
                case 422:
                    CreateResponseUnprocessableEntity(message);
                    break;
                case 200:
                    CreateResponseOk();
                    break;
                case 201:
                    CreateResponseCreated();
                    break;
                case 202:
                    CreateResponseAccepted();
                    break;
                case 204:
                    CreateResponseNoContent();
                    break;
                case 400:
                    CreateResponseBadRequest(message);
                    break;
                case 500:
                    CreateResponseInternalServerError(message);
                    break;
                case 403:
                    CreateResponseForbidden();
                    break;
                case 404:
                    CreateResponseNotFound();
                    break;
                case 503:
                    CreateResponseServiceUnavailable(message);
                    break;
                case 401:
                    CreateResponseUnauthorized(message);
                    break;
                case 406:
                    CreateResponseNotAcceptable();
                    break;
                case 408:
                    CreateResponseRequestTimeout();
                    break;
                case 504:
                    CreateResponseGatewayTimeout();
                    break;
                default:
                    CreateResponseInternalServerError(message);
                    break;
            }
        }

        async Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            if ((int)HttpStatusCode == 204)            {
                var objectResult = new ObjectResult(null) { StatusCode = (int)HttpStatusCode };
                await objectResult.ExecuteResultAsync(context);
            }
            else
            {
                var objectResult = new ObjectResult(this) { StatusCode = (int)HttpStatusCode };
                await objectResult.ExecuteResultAsync(context);
            }
        }
    }

    [Serializable]
    public class ResultResponseMessage<TResponseMessage> : ResultResponseMessage, IActionResult
        where TResponseMessage : ResponseMessage
    {
        public ResultResponseMessage() : base() { }

        public ResultResponseMessage(RequestMessage requestMessage) : base(requestMessage) { }

        [JsonPropertyName("return")]
        public TResponseMessage Return { get; private set; }

        public void SetReturn(TResponseMessage responseMessage) => Return = responseMessage;

        async Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            var objectResult = new ObjectResult(this) { StatusCode = (int)HttpStatusCode };

            SetCustomHeaders(context);

            await objectResult.ExecuteResultAsync(context);
        }

        private void SetCustomHeaders(ActionContext context)
        {
            if (Return == null)
                return;

            foreach (var defaultHeader in Return.DefaultResponseHeaders)
            {
                if (!string.IsNullOrWhiteSpace(defaultHeader.Value))
                    context.HttpContext.Response.Headers.Add(defaultHeader.Key, defaultHeader.Value);
            }

            context.HttpContext.Response.Headers.Add(Headers.Protocol, Protocol);
        }
    }
}
