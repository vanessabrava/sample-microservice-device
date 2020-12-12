using Sample.Microservice.Device.App.Messages;
using Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules;
using System;
using Model = Sample.Microservice.Device.Service.Model;

namespace Sample.Microservice.Device.App.Mapper
{
    public static class DeviceMapper
    {
        public static IModelResult<Model.Device> FromMessage(DeviceMessage message)
        {
            if (message == null)
                return default;

            return Model.Device.New(message.DeviceCode, message.DeviceRegion, message.CaptureDate.GetValueOrDefault(DateTime.MinValue));
        }
    }
}
