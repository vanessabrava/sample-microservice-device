using Microsoft.AspNetCore.Mvc;
using Sample.Microservice.Device.Infra.CrossCutting.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sample.Microservice.Device.App.Messages
{
    [Serializable]
    public class DeviceRequestMessage : RequestMessage
    {
        [BindProperty(Name = "device"), JsonPropertyName("device"), Required]
        public DeviceMessage DeviceMessage { get; set; }
    }
}
