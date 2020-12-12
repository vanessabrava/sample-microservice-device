namespace Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules
{
    public interface IValidation
    {
        string Attribute { get; }

        string Message { get; }
    }
}
