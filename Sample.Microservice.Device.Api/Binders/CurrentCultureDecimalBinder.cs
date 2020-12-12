using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Api.Binders
{
    internal class CurrentCultureDecimalBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            decimal.TryParse(value.FirstValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal decimalValue);

            bindingContext.Result = ModelBindingResult.Success(decimalValue);
            return Task.CompletedTask;
        }
    }
}