using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules
{
    public class ModelResult : IModelResult
    {
        public ModelResult()
        {
            ModelStatusCode = 200;
            Message = "Success.";
        }

        public ModelResult(Guid protocol) : this() => Protocol = protocol.ToString("N");

        public string Protocol { get; private set; }

        public int ModelStatusCode { get; private set; }

        public string Message { get; private set; }

        public bool IsModelResultValid() => ModelStatusCode < 400;

        public virtual void CreateOk()
        {
            Message = "Success.";
            ModelStatusCode = 200;
        }

        public void CreateBusinessValidationResult(string message)
        {
            Message = message;
            ModelStatusCode = 422;
        }

        public void CreateErrorValidationResult(string message = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? Messages.ErrorDefault : message;
            ModelStatusCode = 500;
        }
    }

    public class ModelResult<TModel> : ModelResult, IModelResult<TModel>
        where TModel : IModel
    {

        public ModelResult()
            : base()
        {
        }

        public ModelResult(Guid protocol)
            : base(protocol)
        {
        }

        public TModel Model { get; private set; }

        public IEnumerable<IValidation> Validations { get; private set; } = new List<IValidation>();

        public void AddValidation(string attribute, string message)
        {
            var validations = new List<IValidation>();

            if (Validations.Any())
                foreach (var validation in Validations)
                    validations.Add(validation);

            validations.Add(new Validation(attribute, message));

            Validations = validations;

            CreateBusinessValidationResult("Validation.");
        }

        public void SetModel(TModel model) => Model = model;

        public override void CreateOk()
        {
            if (!Validations.Any())
                base.CreateOk();
        }
    }
}