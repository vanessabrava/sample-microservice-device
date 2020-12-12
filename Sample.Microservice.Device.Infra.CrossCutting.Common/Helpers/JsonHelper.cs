using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Sample.Microservice.Device.Infra.CrossCutting.Common.Helpers
{
    public class JsonHelper
    {
        public static string MaskSensitiveData(string json, List<string> sensitiveProperties)
        {
            if (!IsValidJson(json))
                return json;

            object deserializedValue = System.Text.Json.JsonSerializer.Deserialize<object>(json);

            if (!(deserializedValue is JsonElement))
                return json;

            return RecursiveSensitiveDataMask(((JsonElement)deserializedValue).EnumerateObject(), sensitiveProperties);
        }

        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.StartsWith("---") || !((json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]"))))
                return false;

            try
            {
                json = json.Trim();
                var obj = JsonDocument.Parse(json);
                return true;
            }
            catch { return false; }

        }

        private static string RecursiveSensitiveDataMask(IEnumerable<JsonProperty> enumerateObject, IEnumerable<string> sensitiveProperties)
        {
            if (!enumerateObject.Any())
                return string.Empty;

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();

                foreach (var jsonProperty in enumerateObject)
                {
                    if (jsonProperty.Value.ValueKind == JsonValueKind.Null)
                        continue;

                    if (jsonProperty.Value.ValueKind == JsonValueKind.Object)
                    {
                        writer.WritePropertyName(jsonProperty.Name);
                        RecursiveSensitiveDataMask(writer, jsonProperty.Value.EnumerateObject(), sensitiveProperties);
                    }
                    else
                    {
                        if (sensitiveProperties.Any(it => it.ToLower() == jsonProperty.Name.ToLower()))
                            writer.WriteString(jsonProperty.Name, "***");
                        else
                            jsonProperty.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static void RecursiveSensitiveDataMask(Utf8JsonWriter writer, IEnumerable<JsonProperty> enumerateObject, IEnumerable<string> sensitiveProperties)
        {
            if (!enumerateObject.Any())
                return;

            writer.WriteStartObject();

            foreach (var jsonProperty in enumerateObject)
            {
                if (jsonProperty.Value.ValueKind == JsonValueKind.Null)
                    continue;

                if (jsonProperty.Value.ValueKind == JsonValueKind.Object)
                {
                    writer.WritePropertyName(jsonProperty.Name);
                    RecursiveSensitiveDataMask(writer, jsonProperty.Value.EnumerateObject(), sensitiveProperties);
                }
                else
                {
                    if (sensitiveProperties.Any(it => it.ToLower() == jsonProperty.Name.ToLower()))
                        writer.WriteString(jsonProperty.Name, "***");
                    else
                        jsonProperty.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
    }
}