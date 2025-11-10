using BlazeJump.Tools.Models.Crypto;
using NBitcoin.Secp256k1;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Provides cryptographic operations for Nostr protocol.
	/// </summary>
	public interface ICryptoService
	{
		/// <summary>
		/// Gets the ethereal public key used for temporary encryption.
		/// </summary>
		ECPubKey? EtherealPublicKey { get; }

		/// <summary>
		/// Gets the permanent public key used for persistent identity.
		/// </summary>
		ECPubKey? PermanentPublicKey { get; }

		/// <summary>
		/// Gets the permanent public key as a hex string.
		/// </summary>
		string? PermanentPublicKeyHex { get; }

		/// <summary>
		/// Creates a new ethereal key pair for temporary encryption.
		/// </summary>
		void CreateEtherealKeyPair();

		/// <summary>
		/// Creates or loads a permanent key pair for persistent identity.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task CreateOrLoadPermanentKeyPair();

		/// <summary>
		/// Saves the permanent key pair to secure storage.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SavePermanentKeyPair();

		/// <summary>
		/// Loads the permanent key pair from secure storage.
		/// </summary>
		/// <returns>True if a key pair was loaded; otherwise, false.</returns>
		Task<bool> LoadPermanentKeyPair();

		/// <summary>
		/// Deletes the permanent key pair from secure storage.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeletePermanentKeyPair();

		/// <summary>
		/// Generates a new Secp256k1 key pair.
		/// </summary>
		/// <returns>A new Secp256k1 key pair.</returns>
		Secp256k1KeyPair GetNewSecp256k1KeyPair();

		/// <summary>
		/// Encrypts plain text using AES encryption with the provided public key.
		/// </summary>
		/// <param name="plainText">The text to encrypt.</param>
		/// <param name="theirPublicKey">The recipient's public key.</param>
		/// <param name="ivOverride">Optional initialization vector override.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The encrypted cipher text and IV.</returns>
		Task<CipherIv> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null, bool ethereal = true);

		/// <summary>
		/// Decrypts base64 cipher text using AES decryption.
		/// </summary>
		/// <param name="base64CipherText">The base64-encoded cipher text.</param>
		/// <param name="theirPublicKey">The sender's public key.</param>
		/// <param name="ivString">The initialization vector.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The decrypted plain text.</returns>
		Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString, bool ethereal = true);

		/// <summary>
		/// Signs a message using Schnorr signature.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The signature as a hex string.</returns>
		Task<string> Sign(string message, bool ethereal = true);

		/// <summary>
		/// Verifies a Schnorr signature.
		/// </summary>
		/// <param name="signature">The signature to verify.</param>
		/// <param name="message">The signed message.</param>
		/// <param name="publicKey">The signer's public key.</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		bool Verify(string signature, string message, string publicKey);
	}
}