using BlazeJump.Tools.Models.Crypto;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Provides cryptographic operations for Nostr protocol.
	/// </summary>
	public interface ICryptoService
	{
		/// <summary>
		/// Generates a new cryptographic key pair for Nostr protocol.
		/// </summary>
		/// <returns>The generated public key.</returns>
		Task<string> GenerateKeyPair(string? privateKey = null);
		
		/// <summary>
		/// Encrypts plain text using NIP-44 encryption.
		/// </summary>
		/// <param name="plainText">The text to encrypt.</param>
		/// <param name="theirPublicKey">The recipient's public key.</param>
		/// <param name="ourPublicKey">Our public key.</param>
		/// <returns>The base64-encoded encrypted payload.</returns>
		Task<string> Nip44Encrypt(string plainText, string theirPublicKey, string ourPublicKey);

		/// <summary>
		/// Decrypts NIP-44 encrypted text.
		/// </summary>
		/// <param name="base64Payload">The base64-encoded encrypted payload.</param>
		/// <param name="theirPublicKey">The sender's public key.</param>
		/// <param name="ourPublicKey">Our public key.</param>
		/// <returns>The decrypted plain text.</returns>
		Task<string> Nip44Decrypt(string base64Payload, string theirPublicKey, string ourPublicKey);

		/// <summary>
		/// Encrypts plain text using NIP-04 encryption (AES-256-CBC).
		/// </summary>
		Task<string> Nip04Encrypt(string plainText, string theirPublicKey, string ourPublicKey);

		/// <summary>
		/// Decrypts NIP-04 encrypted text.
		/// </summary>
		Task<string> Nip04Decrypt(string payload, string theirPublicKey, string ourPublicKey);

		/// <summary>
		/// Signs a message using Schnorr signature.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <param name="ourPublicKey">Our public key.</param>
		/// <returns>The signature as a hex string.</returns>
		Task<string> Sign(string message, string ourPublicKey);

		/// <summary>
		/// Verifies a Schnorr signature.
		/// </summary>
		/// <param name="signature">The signature to verify.</param>
		/// <param name="message">The signed message.</param>
		/// <param name="publicKey">The signer's public key.</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		bool Verify(string signature, string message, string publicKey);

        /// <summary>
        /// Gets the existing public key from persistent storage, if available.
        /// </summary>
        /// <returns>The public key as a hex string, or null if none exists.</returns>
        Task<string?> GetExistingPublicKey();

        /// <summary>
        /// Removes the stored key pair from persistent storage.
        /// </summary>
        Task RemoveKeyPair();
	}
}