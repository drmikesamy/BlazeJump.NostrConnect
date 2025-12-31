using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Tools.Services.Crypto;
using System.Text.Json;

namespace NostrConnect.Maui.Services.Crypto
{
	public class NativeCryptoService : CryptoService
	{
		/// <summary>
		/// Gets existing Secp256k1 private key from localstorage.
		/// </summary>
		/// <returns>Existing Secp256k1 private key</returns>
		protected override async Task<string?> GetPrivateKey(string pubkey)
		{
			return await SecureStorage.Default.GetAsync($"blazejumpuserkeypair_{pubkey}");
		}

		/// <summary>
		/// Generates a new Secp256k1 key pair and stores it in secure storage.
		/// </summary>
		/// <returns>A new Secp256k1 key pair.</returns>
		protected override async Task<Secp256k1KeyPair> GenerateSecp256k1KeyPair(string? privateKey = null)
		{
			var newKeyPair = await base.GenerateSecp256k1KeyPair();
			await SecureStorage.Default.SetAsync($"blazejumpuserkeypair_{newKeyPair.PublicKey}", newKeyPair.PrivateKey);
			return newKeyPair;
		}
	}
}