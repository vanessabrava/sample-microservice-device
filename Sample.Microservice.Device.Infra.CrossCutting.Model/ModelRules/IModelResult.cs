using System.Collections.Generic;

namespace Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules
{
    public interface IModelResult
    {
        string Protocol { get; }

        int ModelStatusCode { get; }

        string Message { get; }

        bool IsModelResultValid();

    }

    public interface IModelResult<TModel> : IModelResult
        where TModel : IModel
    {
        TModel Model { get; }

        IEnumerable<IValidation> Validations { get; }
    }
}
