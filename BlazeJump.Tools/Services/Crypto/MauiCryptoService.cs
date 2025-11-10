using BlazeJump.Tools.Models.Crypto;
using NBitcoin.Secp256k1;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// MAUI-specific implementation of CryptoService with secure storage for permanent keys.
	/// This class uses platform-specific secure storage to persist the permanent key pair.
	/// </summary>
	public class MauiCryptoService : CryptoService
	{
		private const string PERMANENT_PRIVATE_KEY = "nostr_permanent_private_key";
		private const string PERMANENT_PUBLIC_KEY = "nostr_permanent_public_key";

		/// <summary>
		/// Initializes a new instance of the <see cref="MauiCryptoService"/> class.
		/// </summary>
		/// <param name="browserCrypto">The browser crypto service for AES operations.</param>
		public MauiCryptoService(IBrowserCrypto? browserCrypto = null) : base(browserCrypto)
		{
		}

		/// <summary>
		/// Saves the permanent key pair to secure storage.
		/// Uses platform-specific secure storage APIs (Keychain on iOS, KeyStore on Android).
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public override async Task SavePermanentKeyPair()
		{
			if (_permanentKeyPair == null)
				throw new InvalidOperationException("No permanent key pair to save.");

#if ANDROID || IOS || MACCATALYST || WINDOWS
			try
			{
				var privateKeyHex = Convert.ToHexString(_permanentKeyPair.PrivateKey.sec.ToBytes()).ToLower();
				var publicKeyHex = Convert.ToHexString(_permanentKeyPair.PublicKey.ToXOnlyPubKey().ToBytes()).ToLower();

				await SecureStorage.SetAsync(PERMANENT_PRIVATE_KEY, privateKeyHex);
				await SecureStorage.SetAsync(PERMANENT_PUBLIC_KEY, publicKeyHex);
			}
			catch (Exception ex)
			{
				// Handle platform-specific exceptions
				throw new InvalidOperationException($"Failed to save permanent key pair: {ex.Message}", ex);
			}
#else
			// For non-MAUI platforms, do nothing
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Loads the permanent key pair from secure storage.
		/// </summary>
		/// <returns>True if a key pair was loaded; otherwise, false.</returns>
		public override async Task<bool> LoadPermanentKeyPair()
		{
#if ANDROID || IOS || MACCATALYST || WINDOWS
			try
			{
				var privateKeyHex = await SecureStorage.GetAsync(PERMANENT_PRIVATE_KEY);
				
				if (string.IsNullOrEmpty(privateKeyHex))
					return false;

				var privateKeyBytes = Convert.FromHexString(privateKeyHex);
				var privateKey = ECPrivKey.Create(privateKeyBytes);
				var publicKey = privateKey.CreatePubKey();

				_permanentKeyPair = new Secp256k1KeyPair(privateKey, publicKey);
				return true;
			}
			catch (Exception)
			{
				// Key not found or invalid
				return false;
			}
#else
			return await Task.FromResult(false);
#endif
		}

		/// <summary>
		/// Deletes the permanent key pair from secure storage.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public override async Task DeletePermanentKeyPair()
		{
			_permanentKeyPair = null;

#if ANDROID || IOS || MACCATALYST || WINDOWS
			try
			{
				SecureStorage.Remove(PERMANENT_PRIVATE_KEY);
				SecureStorage.Remove(PERMANENT_PUBLIC_KEY);
			}
			catch (Exception)
			{
				// Ignore errors during deletion
			}
#endif
			await Task.CompletedTask;
		}
	}
}
