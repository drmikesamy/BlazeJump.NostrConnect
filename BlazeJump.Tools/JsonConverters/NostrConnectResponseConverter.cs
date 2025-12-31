using BlazeJump.Tools.Models.NostrConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazeJump.Tools.JsonConverters
{
    /// <summary>
    /// JSON converter for NostrConnectResponse that ensures proper serialization
    /// with clean handling of result values (strings, objects, or arrays).
    /// </summary>
    public class NostrConnectResponseConverter : JsonConverter<NostrConnectResponse>
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        public override NostrConnectResponse ReadJson(JsonReader reader, Type objectType, NostrConnectResponse? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            
            var response = new NostrConnectResponse
            {
                Id = jObject["id"]?.Value<string>() ?? string.Empty,
                Error = jObject["error"]?.Value<string>() ?? string.Empty
            };

            // Parse result - can be a string, object, or array
            var resultToken = jObject["result"];
            if (resultToken != null)
            {
                response.Result = resultToken.Type switch
                {
                    JTokenType.String => resultToken.Value<string>() ?? string.Empty,
                    JTokenType.Object or JTokenType.Array => resultToken.ToString(Formatting.None),
                    JTokenType.Null => string.Empty,
                    _ => resultToken.ToString()
                };
            }

            return response;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, NostrConnectResponse? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            // Write id
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);

            // Write result - handle as raw JSON if it's a JSON structure, otherwise as string
            writer.WritePropertyName("result");
            if (!string.IsNullOrEmpty(value.Result) && IsJsonStructure(value.Result))
            {
                writer.WriteRawValue(value.Result);
            }
            else
            {
                writer.WriteValue(value.Result);
            }

            // Write error
            writer.WritePropertyName("error");
            writer.WriteValue(value.Error);

            writer.WriteEndObject();
        }

        private static bool IsJsonStructure(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            var trimmed = str.Trim();
            return (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                   (trimmed.StartsWith("[") && trimmed.EndsWith("]"));
        }
    }
}
