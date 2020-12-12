using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sample.Microservice.Device.Infra.CrossCutting.Services
{
    [Serializable]
    public class ResponseMessage
    {
        [JsonIgnore]
        public virtual IDictionary<string, string> DefaultResponseHeaders { get; private set; } = new Dictionary<string, string>();

        public virtual void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (DefaultResponseHeaders.Contains(new KeyValuePair<string, string>(key, value)))
                return;

            if (!DefaultResponseHeaders.ContainsKey(key))
                DefaultResponseHeaders.Add(key, value);
            else
                DefaultResponseHeaders[key] = value;
        }

        public virtual string GetHeader(string key) => DefaultResponseHeaders.TryGetValue(key, out string outValue) ? outValue : string.Empty;
    }
}