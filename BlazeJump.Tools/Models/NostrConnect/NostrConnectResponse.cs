using BlazeJump.Tools.JsonConverters;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Models.NostrConnect;

/// <summary>
/// Represents a response in the Nostr Connect protocol (NIP-46).
/// </summary>
[JsonConverter(typeof(NostrConnectResponseConverter))]
public class NostrConnectResponse
{
    /// <summary>
    /// Gets or sets the request ID this response corresponds to.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the result of the request.
    /// For connect responses, this is "ack" or the secret.
    /// For sign_event, this is the JSON stringified signed event.
    /// For ping, this is "pong".
    /// For get_public_key, this is the user's public key.
    /// For encryption/decryption, this is the ciphertext or plaintext.
    /// </summary>
    public string Result { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the error message if the request failed. Empty if successful.
    /// </summary>
    public string Error { get; set; } = string.Empty;
}
