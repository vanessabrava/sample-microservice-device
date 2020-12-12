using System;
using System.Text.Json.Serialization;

namespace Sample.Microservice.Device.Infra.CrossCutting.Services
{
    [Serializable]
    public class ValidationMessage
    {
        [JsonPropertyName("attribute")]
        public string Attribute { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
