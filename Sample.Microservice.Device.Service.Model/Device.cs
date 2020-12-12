using Sample.Microservice.Device.Infra.CrossCutting.Model;
using Sample.Microservice.Device.Infra.CrossCutting.Model.ModelRules;
using System;

namespace Sample.Microservice.Device.Service.Model
{
    public class Device : IAggregateRoot
    {
        private Device() { }

        public Device(string deviceCode, string deviceRegion, DateTime captureDate)
        {
            DeviceCode = deviceCode;
            DeviceRegion = deviceRegion;
            CaptureDate = captureDate;
            Date = DateTime.Now.ToUniversalTime();
        }

        public long DeviceId { get; private set; }

        public string DeviceCode { get; private set; }

        public string DeviceRegion { get; private set; }

        public DateTime CaptureDate { get; private set; }

        public DateTime Date { get; private set; }

        public static IModelResult<Device> New(string deviceCode, string deviceRegion, DateTime captureDate)
        {
            var modelResult = new ModelResult<Device>();

            var model = new Device(deviceCode, deviceRegion, captureDate);

            model.Validate(modelResult);

            if (modelResult.IsModelResultValid())
                modelResult.SetModel(model);

            return modelResult;
        }

        private void Validate(ModelResult<Device> modelResult)
        {
            if (!modelResult.IsModelResultValid())
                return;

            if (string.IsNullOrWhiteSpace(DeviceCode))
                modelResult.AddValidation("DeviceCode", "The DeviceCode must be informed.");

            if (string.IsNullOrWhiteSpace(DeviceRegion))
                modelResult.AddValidation("DeviceRegion", "The DeviceRegion must be informed.");

            if (CaptureDate == DateTime.MinValue)
                modelResult.AddValidation("CaptureDate", "The CaptureDate must be informed.");
        }
    }
}
