using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models.NostrConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazeJump.Tools.JsonConverters
{
    /// <summary>
    /// JSON converter for NostrConnectRequest that ensures proper serialization
    /// with lowercase snake_case commands and array formatting.
    /// </summary>
    public class NostrConnectRequestConverter : JsonConverter<NostrConnectRequest>
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        public override NostrConnectRequest ReadJson(JsonReader reader, Type objectType, NostrConnectRequest? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            
            var request = new NostrConnectRequest
            {
                Id = jObject["id"]?.Value<string>() ?? string.Empty
            };

            // Parse method/command
            var methodStr = jObject["method"]?.Value<string>();
            if (!string.IsNullOrEmpty(methodStr))
            {
                request.Method = ParseCommand(methodStr);
            }

            // Parse params array
            var paramsToken = jObject["params"];
            if (paramsToken != null && paramsToken.Type == JTokenType.Array)
            {
                var paramsArray = paramsToken.ToObject<JArray>();
                if (paramsArray != null)
                {
                    request.Params = paramsArray
                        .Select(token => token.Type == JTokenType.String 
                            ? token.Value<string>() ?? string.Empty
                            : token.ToString(Formatting.None))
                        .ToArray();
                }
            }

            return request;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, NostrConnectRequest? value, JsonSerializer serializer)
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

            // Write method as lowercase snake_case
            writer.WritePropertyName("method");
            writer.WriteValue(CommandToString(value.Method));

            // Write params as array
            writer.WritePropertyName("params");
            writer.WriteStartArray();
            
            if (value.Params != null)
            {
                foreach (var param in value.Params)
                {
                    // Try to determine if param is a JSON object/array or plain string
                    if (IsJsonStructure(param))
                    {
                        writer.WriteRawValue(param);
                    }
                    else
                    {
                        writer.WriteValue(param);
                    }
                }
            }
            
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        private static CommandEnum ParseCommand(string command)
        {
            return command.ToLowerInvariant() switch
            {
                "connect" => CommandEnum.Connect,
                "sign_event" => CommandEnum.SignEvent,
                "ping" => CommandEnum.Ping,
                "get_public_key" => CommandEnum.GetPublicKey,
                "nip04_encrypt" => CommandEnum.Nip04Encrypt,
                "nip04_decrypt" => CommandEnum.Nip04Decrypt,
                "nip44_encrypt" => CommandEnum.Nip44Encrypt,
                "nip44_decrypt" => CommandEnum.Nip44Decrypt,
                "disconnect" => CommandEnum.Disconnect,
                _ => CommandEnum.Connect
            };
        }

        private static string CommandToString(CommandEnum command)
        {
            return command switch
            {
                CommandEnum.Connect => "connect",
                CommandEnum.SignEvent => "sign_event",
                CommandEnum.Ping => "ping",
                CommandEnum.GetPublicKey => "get_public_key",
                CommandEnum.Nip04Encrypt => "nip04_encrypt",
                CommandEnum.Nip04Decrypt => "nip04_decrypt",
                CommandEnum.Nip44Encrypt => "nip44_encrypt",
                CommandEnum.Nip44Decrypt => "nip44_decrypt",
                CommandEnum.Disconnect => "disconnect",
                _ => "connect"
            };
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
