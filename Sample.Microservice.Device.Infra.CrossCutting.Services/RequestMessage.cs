using Sample.Microservice.Device.Infra.CrossCutting.Common.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sample.Microservice.Device.Infra.CrossCutting.Services
{
    [Serializable]
    public class RequestMessage
    {
        public RequestMessage() { }

        [JsonIgnore]
        public string Protocol => GetHeader(Headers.Protocol);

        [JsonIgnore]
        public IDictionary<string, string> DefaultRequestHeaders { get; private set; } = new Dictionary<string, string>();

        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (DefaultRequestHeaders.Contains(new KeyValuePair<string, string>(key, value)))
                return;

            if (!DefaultRequestHeaders.ContainsKey(key))
                DefaultRequestHeaders.Add(key, value);
            else
                DefaultRequestHeaders[key] = value;
        }

        public string GetHeader(string key) => DefaultRequestHeaders.TryGetValue(key, out string outValue) ? outValue : string.Empty;
    }
}
