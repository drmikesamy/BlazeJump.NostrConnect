using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Helpers;
using System.Text.Json;
using BlazeJump.Tools.Helpers;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Implements cryptographic operations for Nostr protocol using Secp256k1 and ChaCha20 encryption.
	/// Uses BouncyCastle for all cryptographic operations.
	/// </summary>
	public abstract class CryptoService : ICryptoService
	{
		/// <summary>
		/// Generates a new cryptographic key pair for Nostr protocol.
		/// </summary>
		/// <returns>The generated public key.</returns>
		public async Task<string> GenerateKeyPair(string? privateKey = null)
		{
			return (await GenerateSecp256k1KeyPair(privateKey)).PublicKey;
		}

		/// <summary>
		/// Gets existing Secp256k1 private key from localstorage.
		/// </summary>
		/// <returns>Existing Secp256k1 private key</returns>
		protected virtual async Task<string?> GetPrivateKey(string pubkey)
		{
			return default;
		}

		/// <summary>
		/// Generates a new Secp256k1 key pair.
		/// </summary>
		protected virtual async Task<Secp256k1KeyPair> GenerateSecp256k1KeyPair(string? privateKey = null)
		{
			byte[] privateKeyBytes = default!;
			if (!string.IsNullOrEmpty(privateKey))
			{
				if (privateKey.ToLower().StartsWith(Enums.Bech32PrefixEnum.nsec.ToString()))
				{
					privateKey = GeneralHelpers.Bech32ToHex(privateKey, Enums.Bech32PrefixEnum.nsec);
				}

				privateKeyBytes = Convert.FromHexString(privateKey);

				if (!Secp256k1Crypto.IsValidPrivateKey(privateKeyBytes))
					throw new ArgumentException("Invalid private key");
			}
			else
			{
				privateKeyBytes = Secp256k1Crypto.GeneratePrivateKey();
			}

			var publicKeyBytes = Secp256k1Crypto.GetXOnlyPublicKey(privateKeyBytes);

			var privateKeyString = Convert.ToHexString(privateKeyBytes).ToLower();
			var publicKeyString = Convert.ToHexString(publicKeyBytes).ToLower();

			return new Secp256k1KeyPair(privateKeyString, publicKeyString);
		}

		/// <summary>
		/// Gets the shared secret for ECDH key exchange.
		/// Uses BouncyCastle implementation.
		/// </summary>
		/// <param name="theirPublicKey">The other party's public key (hex string).</param>
		/// <param name="ourPublicKey">Our active public key for lookup of private key (hex string).</param>
		/// <returns>The shared secret bytes (32-byte x-coordinate).</returns>
		protected async Task<byte[]> GetSharedSecret(string theirPublicKey, string ourPublicKey)
		{
			var theirPublicKeyBytes = Convert.FromHexString(theirPublicKey);

			// Handle 32-byte x-only public keys (Nostr format)
			if (theirPublicKeyBytes.Length == 32)
			{
				// Try with 0x02 prefix (even y-coordinate)
				var compressedBytes = new byte[33];
				compressedBytes[0] = 0x02;
				Array.Copy(theirPublicKeyBytes, 0, compressedBytes, 1, 32);

				if (!Secp256k1Crypto.IsValidPublicKey(compressedBytes))
				{
					// Try with 0x03 prefix (odd y-coordinate)
					compressedBytes[0] = 0x03;
				}
				theirPublicKeyBytes = compressedBytes;
			}
			else if (theirPublicKeyBytes.Length != 33 && theirPublicKeyBytes.Length != 65)
			{
				throw new ArgumentException($"Invalid pubkey length: {theirPublicKeyBytes.Length}. Expected 32, 33, or 65 bytes.");
			}

			var privateKey = await GetPrivateKey(ourPublicKey);
			if (string.IsNullOrEmpty(privateKey))
			{
				throw new Exception("Private key does not exist in storage!");
			}
			var privateKeyBytes = Convert.FromHexString(privateKey);
			var sharedSecretXOnly = Secp256k1Crypto.ComputeSharedSecretXOnly(privateKeyBytes, theirPublicKeyBytes);
			return sharedSecretXOnly;
			throw new Exception("Active key pair not found or private key is null.");
		}

		/// <summary>
		/// Encrypts plain text using NIP-44 encryption with ChaCha20.
		/// </summary>
		public virtual async Task<string> Nip44Encrypt(string plainText, string theirPublicKey, string ourPublicKey)
		{
			try
			{
				byte[] sharedSecret = await GetSharedSecret(theirPublicKey, ourPublicKey);
				byte[] conversationKey = Nip44.DeriveConversationKey(sharedSecret);
				return Nip44.Encrypt(plainText, conversationKey);
			}
			catch (Exception ex)
			{
				throw new Exception($"Nip44Encrypt failed for pubkey {theirPublicKey} with user pubkey {ourPublicKey}: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Decrypts NIP-44 encrypted text using ChaCha20.
		/// </summary>
		public virtual async Task<string> Nip44Decrypt(string base64Payload, string theirPublicKey, string ourPublicKey)
		{
			try
			{
				byte[] sharedSecret = await GetSharedSecret(theirPublicKey, ourPublicKey);
				byte[] conversationKey = Nip44.DeriveConversationKey(sharedSecret);
				return Nip44.Decrypt(base64Payload, conversationKey);
			}
			catch (Exception ex)
			{
				throw new Exception($"Nip44Decrypt failed for pubkey {theirPublicKey}: {ex.Message}", ex);
			}
		}

		public virtual async Task<string> Nip04Encrypt(string plainText, string theirPublicKey, string ourPublicKey)
		{
			try
			{
				byte[] sharedSecret = await GetSharedSecret(theirPublicKey, ourPublicKey);
				return Nip04.Encrypt(plainText, sharedSecret);
			}
			catch (Exception ex)
			{
				throw new Exception($"Nip04Encrypt failed: {ex.Message}", ex);
			}
		}

		public virtual async Task<string> Nip04Decrypt(string payload, string theirPublicKey, string ourPublicKey)
		{
			try
			{
				byte[] sharedSecret = await GetSharedSecret(theirPublicKey, ourPublicKey);
				return Nip04.Decrypt(payload, sharedSecret);
			}
			catch (Exception ex)
			{
				throw new Exception($"Nip04Decrypt failed: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Signs a message using Schnorr signature (BIP-340).
		/// </summary>
		/// <param name="message">The message to sign.</param>
		/// <param name="ourPublicKey">The pubkey corresponding to the private key we are using to sign.</param>
		/// <returns>The signature as a hex string.</returns>
		public async Task<string> Sign(string message, string ourPublicKey)
		{
			var privateKey = await GetPrivateKey(ourPublicKey);
			if (string.IsNullOrEmpty(privateKey))
			{
				throw new Exception("Private key does not exist in storage!");
			}
			var messageHashBytes = message.SHA256Hash();
			var privateKeyBytes = Convert.FromHexString(privateKey);
			var signature = Secp256k1Crypto.SignSchnorr(messageHashBytes, privateKeyBytes);
			return Convert.ToHexString(signature).ToLower();
			throw new Exception("Active key pair not found or private key is null.");
		}

		/// <summary>
		/// Verifies a Schnorr signature (BIP-340).
		/// </summary>
		/// <param name="signature">The signature to verify (hex string).</param>
		/// <param name="message">The signed message.</param>
		/// <param name="publicKey">The signer's public key (hex string, 32-byte x-only).</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		public bool Verify(string signature, string message, string publicKey)
		{
			try
			{
				var messageHashBytes = message.SHA256Hash();
				var signatureBytes = Convert.FromHexString(signature);
				var publicKeyBytes = Convert.FromHexString(publicKey);

				return Secp256k1Crypto.VerifySchnorr(messageHashBytes, signatureBytes, publicKeyBytes);
			}
			catch
			{
				return false;
			}
		}

        /// <summary>
        /// Gets the existing public key from persistent storage.
        /// Base implementation returns null (no storage).
        /// </summary>
        public virtual Task<string?> GetExistingPublicKey()
        {
            return Task.FromResult<string?>(null);
        }

        /// <summary>
        /// Removes the stored key pair.
        /// Base implementation does nothing.
        /// </summary>
        public virtual Task RemoveKeyPair()
        {
            return Task.CompletedTask;
        }
	}
}
