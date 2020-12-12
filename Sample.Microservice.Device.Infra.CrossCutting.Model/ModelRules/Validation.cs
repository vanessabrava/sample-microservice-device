namespace Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules
{
    public class Validation : IValidation
    {
        public Validation(string attribute, string message)
        {
            Attribute = attribute;
            Message = message;
        }

        public string Attribute { get; }

        public string Message { get; }
    }
}