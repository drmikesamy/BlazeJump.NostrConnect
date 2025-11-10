using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Helpers;
using Microsoft.JSInterop;
using NBitcoin.Secp256k1;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Implements cryptographic operations for Nostr protocol using Secp256k1 and AES encryption.
	/// </summary>
	public partial class CryptoService : ICryptoService
	{
		/// <summary>
		/// Gets the ethereal public key used for temporary encryption.
		/// </summary>
		public ECPubKey? EtherealPublicKey => _etherealKeyPair?.PublicKey;

		/// <summary>
		/// Gets the permanent public key used for persistent identity.
		/// </summary>
		public ECPubKey? PermanentPublicKey => _permanentKeyPair?.PublicKey;

		/// <summary>
		/// Gets the permanent public key as a hex string.
		/// </summary>
		public string? PermanentPublicKeyHex => _permanentKeyPair?.PublicKey.ToXOnlyPubKey().ToBytes() != null 
			? Convert.ToHexString(_permanentKeyPair.PublicKey.ToXOnlyPubKey().ToBytes()).ToLower() 
			: null;

		/// <summary>
		/// Gets or sets the ethereal key pair.
		/// </summary>
		protected Secp256k1KeyPair? _etherealKeyPair { get; set; }

		/// <summary>
		/// Gets or sets the permanent key pair.
		/// </summary>
		protected Secp256k1KeyPair? _permanentKeyPair { get; set; }

		private readonly IBrowserCrypto? _browserCrypto;

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoService"/> class.
		/// </summary>
		/// <param name="browserCrypto">The browser crypto service for AES operations.</param>
		public CryptoService(IBrowserCrypto? browserCrypto = null)
		{
			_browserCrypto = browserCrypto;
		}

		/// <summary>
		/// Creates a new ethereal key pair for temporary encryption.
		/// </summary>
		public void CreateEtherealKeyPair()
		{
			_etherealKeyPair = GetNewSecp256k1KeyPair();
		}

		/// <summary>
		/// Creates or loads a permanent key pair for persistent identity.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public virtual async Task CreateOrLoadPermanentKeyPair()
		{
			// Try to load existing key pair first
			bool loaded = await LoadPermanentKeyPair();
			
			// If no key pair exists, create a new one
			if (!loaded)
			{
				_permanentKeyPair = GetNewSecp256k1KeyPair();
				await SavePermanentKeyPair();
			}
		}

		/// <summary>
		/// Saves the permanent key pair to secure storage.
		/// Override this method in platform-specific implementations.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public virtual Task SavePermanentKeyPair()
		{
			// Base implementation does nothing (for web apps that don't persist)
			// Override in MAUI/platform-specific implementations
			return Task.CompletedTask;
		}

		/// <summary>
		/// Loads the permanent key pair from secure storage.
		/// Override this method in platform-specific implementations.
		/// </summary>
		/// <returns>True if a key pair was loaded; otherwise, false.</returns>
		public virtual Task<bool> LoadPermanentKeyPair()
		{
			// Base implementation returns false (for web apps that don't persist)
			// Override in MAUI/platform-specific implementations
			return Task.FromResult(false);
		}

		/// <summary>
		/// Deletes the permanent key pair from secure storage.
		/// Override this method in platform-specific implementations.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public virtual Task DeletePermanentKeyPair()
		{
			_permanentKeyPair = null;
			// Override in MAUI/platform-specific implementations to delete from storage
			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates a new Secp256k1 key pair.
		/// </summary>
		/// <returns>A new Secp256k1 key pair.</returns>
		public Secp256k1KeyPair GetNewSecp256k1KeyPair()
		{
			Random rand = new Random();
			byte[] privateKeyGen = new byte[32];
			rand.NextBytes(privateKeyGen);
			var privateKey = ECPrivKey.Create(privateKeyGen);
			var publicKey = privateKey.CreatePubKey();
			return new Secp256k1KeyPair(privateKey, publicKey);
		}

		/// <summary>
		/// Encrypts plain text using AES encryption with the provided public key.
		/// </summary>
		/// <param name="plainText">The text to encrypt.</param>
		/// <param name="theirPublicKey">The recipient's public key.</param>
		/// <param name="ivOverride">Optional initialization vector override.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The encrypted cipher text and IV.</returns>
		public virtual async Task<CipherIv> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null, bool ethereal = true)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);
			byte[] iv = new byte[16];
			if (ivOverride != null)
			{
				iv = Convert.FromBase64String(ivOverride);
			}
			else
			{
				Random rand = new Random();
				rand.NextBytes(iv);
			}
			var ivString = Convert.ToBase64String(iv);
			var paddedTextBytes = Encoding.UTF8.GetBytes(plainText).Pad();
			var encrypted = await _browserCrypto!.InvokeBrowserCrypto("aesEncrypt", paddedTextBytes, sharedPoint, iv);
			return new CipherIv(encrypted.ToString(), ivString);
		}

		/// <summary>
		/// Decrypts base64 cipher text using AES decryption.
		/// </summary>
		/// <param name="base64CipherText">The base64-encoded cipher text.</param>
		/// <param name="theirPublicKey">The sender's public key.</param>
		/// <param name="ivString">The initialization vector.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The decrypted plain text.</returns>
		public virtual async Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString, bool ethereal = true)
		{
			byte[] sharedPoint = await GetSharedSecret(theirPublicKey, ethereal);
			var sharedPointString = Convert.ToBase64String(sharedPoint);
			var decrypted = await _browserCrypto!.InvokeBrowserCrypto("aesDecrypt", base64CipherText, sharedPointString, ivString);
			return decrypted;
		}

		/// <summary>
		/// Gets the shared secret for ECDH key exchange.
		/// </summary>
		/// <param name="theirPublicKey">The other party's public key.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The shared secret bytes.</returns>
		protected async Task<byte[]> GetSharedSecret(string theirPublicKey, bool ethereal)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);
			var theirPubKey = ECPubKey.Create(theirPublicKeyBytes);
			var ourPrivKey = await GetPrivateKey(ethereal);
			return theirPubKey.GetSharedPubkey(ourPrivKey).ToBytes()[1..];
		}

		/// <summary>
		/// Gets the private key for cryptographic operations.
		/// </summary>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The private key.</returns>
		protected virtual Task<ECPrivKey> GetPrivateKey(bool ethereal)
		{
			if (ethereal)
			{
				if (_etherealKeyPair == null)
					throw new InvalidOperationException("Ethereal key pair not initialized. Call CreateEtherealKeyPair() first.");
				return Task.FromResult(_etherealKeyPair.PrivateKey);
			}
			else
			{
				if (_permanentKeyPair == null)
					throw new InvalidOperationException("Permanent key pair not initialized. Call CreateOrLoadPermanentKeyPair() first.");
				return Task.FromResult(_permanentKeyPair.PrivateKey);
			}
		}

		/// <summary>
		/// Signs a message using Schnorr signature.
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <param name="ethereal">Whether to use the ethereal key pair.</param>
		/// <returns>The signature as a hex string.</returns>
		public async Task<string> Sign(string message, bool ethereal = true)
		{
			var privateKey = await GetPrivateKey(ethereal);
			var messageHashBytes = message.SHA256Hash();
			return Convert.ToHexString(privateKey.SignBIP340(messageHashBytes).ToBytes()).ToLower();
		}

		/// <summary>
		/// Verifies a Schnorr signature.
		/// </summary>
		/// <param name="signature">The signature to verify.</param>
		/// <param name="message">The signed message.</param>
		/// <param name="publicKey">The signer's public key.</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		public bool Verify(string signature, string message, string publicKey)
		{
			var messageHashBytes = message.SHA256Hash();
			var signatureBytes = Convert.FromHexString(signature);
			var publicKeyBytes = Convert.FromHexString(publicKey);
			var pubKey = ECXOnlyPubKey.Create(publicKeyBytes);
			if (SecpSchnorrSignature.TryCreate(signatureBytes, out var schnorrSignature))
			{
				return pubKey.SigVerifyBIP340(schnorrSignature, messageHashBytes);
			}
			return false;
		}
	}

	/// <summary>
	/// Represents encrypted cipher text with its initialization vector.
	/// </summary>
	public class CipherIv
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CipherIv"/> class.
		/// </summary>
		/// <param name="cipherText">The encrypted cipher text.</param>
		/// <param name="iv">The initialization vector.</param>
		public CipherIv(string cipherText, string iv) {
			CipherText = cipherText;
			Iv = iv;
		}

		/// <summary>
		/// Gets or sets the encrypted cipher text.
		/// </summary>
		public string CipherText { get; set; }

		/// <summary>
		/// Gets or sets the initialization vector.
		/// </summary>
		public string Iv { get; set; }
	}
}
