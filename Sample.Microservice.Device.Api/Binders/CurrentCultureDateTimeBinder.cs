using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Sample.Microservice.Device.Api.Binders
{
    internal class CurrentCultureDateTimeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            DateTime.TryParse(value.FirstValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date);

            bindingContext.Result = ModelBindingResult.Success(date);

            return Task.CompletedTask;
        }
    }
}
