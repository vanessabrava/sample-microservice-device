using Sample.Microservice.Device.Infra.CrossCutting.Model;
using Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System.Net;

namespace Sample.Microservice.Device.App.Services.Imp.Extensions
{
    public static class ModelResultExtension
    {
        public static ResultResponseMessage<TResponseMessage> ToResultResponseMessage<TRequestMessage, TResponseMessage, TModel>(this IModelResult<TModel> modelResult, TRequestMessage requestMessage)
            where TRequestMessage : RequestMessage
            where TResponseMessage : ResponseMessage
            where TModel : IModel
        {
            var result = new ResultResponseMessage<TResponseMessage>(requestMessage);

            if (!modelResult.IsModelResultValid())
            {
                if (modelResult.Validations != null)
                    foreach (var validation in modelResult.Validations)
                        result.AddValidation(validation.Attribute, validation.Message);

                result.MapResultResponseMessage((HttpStatusCode)modelResult.ModelStatusCode, modelResult.Message);
            }

            return result;
        }

        public static ResultResponseMessage ToResultResponseMessage<TRequestMessage, TModel>(this IModelResult<TModel> modelResult, TRequestMessage requestMessage)
            where TRequestMessage : RequestMessage
            where TModel : IModel
        {
            var result = new ResultResponseMessage(requestMessage);

            if (!modelResult.IsModelResultValid())
            {
                if (modelResult.Validations != null)
                    foreach (var validation in modelResult.Validations)
                        result.AddValidation(validation.Attribute, validation.Message);

                result.MapResultResponseMessage((HttpStatusCode)modelResult.ModelStatusCode, modelResult.Message);
            }

            return result;
        }

        public static ResultResponseMessage ToResultResponseMessage<TRequestMessage>(this IModelResult modelResult, TRequestMessage requestMessage)
            where TRequestMessage : RequestMessage
        {
            var result = new ResultResponseMessage(requestMessage);

            if (!modelResult.IsModelResultValid())
                result.MapResultResponseMessage((HttpStatusCode)modelResult.ModelStatusCode, modelResult.Message);

            return result;
        }
    }
}
