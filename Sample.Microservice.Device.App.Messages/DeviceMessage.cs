using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sample.Microservice.Device.App.Messages
{
    [Serializable]
    public class DeviceMessage
    {
        [JsonPropertyName("deviceCode"), Required]
        public string DeviceCode { get; set; }

        [JsonPropertyName("deviceRegion"), Required]
        public string DeviceRegion { get; set; }

        [JsonPropertyName("captureDate"), Required]
        public DateTime? CaptureDate { get; set; }
    }
}
