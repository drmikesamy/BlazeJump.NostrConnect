namespace BlazeJump.Tools.Enums
{
    /// <summary>
    /// Nostr Connect command types for remote signer communication.
    /// </summary>
    public enum CommandEnum
    {
        /// <summary>
        /// Connect command - establishes connection with remote signer.
        /// Params: [remote-signer-pubkey, optional_secret, optional_requested_permissions]
        /// Result: "ack" OR required-secret-value
        /// </summary>
        Connect,
        
        /// <summary>
        /// Sign event command - requests remote signer to sign a Nostr event.
        /// Params: [{kind, content, tags, created_at}]
        /// Result: json_stringified(signed_event)
        /// </summary>
        SignEvent,
        
        /// <summary>
        /// Ping command - health check for connection.
        /// Params: []
        /// Result: "pong"
        /// </summary>
        Ping,
        
        /// <summary>
        /// Get public key command - retrieves the user's public key from remote signer.
        /// Params: []
        /// Result: user-pubkey
        /// </summary>
        GetPublicKey,
        
        /// <summary>
        /// NIP-04 encrypt command - encrypts plaintext using NIP-04 encryption.
        /// Params: [third_party_pubkey, plaintext_to_encrypt]
        /// Result: nip04_ciphertext
        /// </summary>
        Nip04Encrypt,
        
        /// <summary>
        /// NIP-04 decrypt command - decrypts ciphertext using NIP-04 decryption.
        /// Params: [third_party_pubkey, nip04_ciphertext_to_decrypt]
        /// Result: plaintext
        /// </summary>
        Nip04Decrypt,
        
        /// <summary>
        /// NIP-44 encrypt command - encrypts plaintext using NIP-44 encryption.
        /// Params: [third_party_pubkey, plaintext_to_encrypt]
        /// Result: nip44_ciphertext
        /// </summary>
        Nip44Encrypt,
        
        /// <summary>
        /// NIP-44 decrypt command - decrypts ciphertext using NIP-44 decryption.
        /// Params: [third_party_pubkey, nip44_ciphertext_to_decrypt]
        /// Result: plaintext
        /// </summary>
        Nip44Decrypt,
        
        /// <summary>
        /// Disconnect command - terminates the remote signer session.
        /// Params: []
        /// Result: secret (for verification)
        /// </summary>
        Disconnect
    }
}
