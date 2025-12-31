using BlazeJump.Tools.Enums;
using BlazeJump.Tools.JsonConverters;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Models.NostrConnect;

/// <summary>
/// Represents a request in the Nostr Connect protocol (NIP-46).
/// </summary>
[JsonConverter(typeof(NostrConnectRequestConverter))]
public class NostrConnectRequest
{
    /// <summary>
    /// Gets or sets the unique request identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the method name. Commands are serialized as lowercase snake_case strings.
    /// Valid commands: connect, sign_event, ping, get_public_key, nip04_encrypt, nip04_decrypt, nip44_encrypt, nip44_decrypt.
    /// </summary>
    public CommandEnum Method { get; set; } = CommandEnum.Connect;
    
    /// <summary>
    /// Gets or sets the parameters array for the request.
    /// Parameters are serialized as a JSON array.
    /// </summary>
    public string[] Params { get; set; } = Array.Empty<string>();
}