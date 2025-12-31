using System.Text.Json;
using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Tools.Services.Crypto;
using Blazored.LocalStorage;
using BlazeJump.Helpers;

namespace NostrConnect.Web.Services.Crypto
{
	public class WebCryptoService : CryptoService
	{
		private ILocalStorageService _localStorage;
        private const string StorageKey = "blazejump_client_keypair";

		public WebCryptoService(ILocalStorageService localStorage)
		{
			_localStorage = localStorage;
		}

		/// <summary>
		/// Gets existing Secp256k1 private key from localstorage.
		/// </summary>
		/// <returns>Existing Secp256k1 private key</returns>
		protected override async Task<string?> GetPrivateKey(string pubkey)
		{
            // We only store one keypair for the web client, so we ignore the pubkey param
			return await _localStorage.GetItemAsync<string>($"blazejumpuserkeypair_{pubkey}");
		}

		/// <summary>
		/// Generates a new Secp256k1 key pair and stores it in localstorage.
		/// </summary>
		/// <returns>A new Secp256k1 key pair.</returns>
		protected override async Task<Secp256k1KeyPair> GenerateSecp256k1KeyPair(string? privateKey = null)
		{
			var newKeyPair = await base.GenerateSecp256k1KeyPair(privateKey);
			await _localStorage.SetItemAsync($"blazejumpuserkeypair_{newKeyPair.PublicKey}", newKeyPair.PrivateKey);
			return newKeyPair;
		}
	}
}