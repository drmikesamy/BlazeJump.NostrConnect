using BlazeJump.Tools.Services.Crypto;
using System.Security.Cryptography;
using System.Text;

namespace BlazeJump.Tools.Tests.Mocks
{
    public class MockCryptoService : ICryptoService
    {
        private readonly Dictionary<string, string> _privateKeyStore = new();

        public Task<string> GenerateKeyPair(string? privateKey = null)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                privateKey = Guid.NewGuid().ToString("N");
            }
            
            // In this mock, pubkey = "pub_" + privateKey (simplified)
            // But to keep it realistic looking, we'll just use a hash
            var pubKey = ComputeHash(privateKey);
            _privateKeyStore[pubKey] = privateKey;
            
            return Task.FromResult(pubKey);
        }

        public Task<string> Nip44Decrypt(string base64Payload, string theirPublicKey, string ourPublicKey)
        {
            // Decrypt: remove "enc:" prefix
            // In real app we'd verify keys, but here we assume success for simplicity
            // or we could encode usage of keys in the payload to verify
            
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));
            if (payload.StartsWith("enc:"))
            {
                return Task.FromResult(payload.Substring(4));
            }
            return Task.FromResult(payload);
        }

        public Task<string> Nip44Encrypt(string plainText, string theirPublicKey, string ourPublicKey)
        {
            // Simple mock encryption: "enc:" + plainText
            var payload = "enc:" + plainText;
            return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)));
        }

        public Task<string> Nip04Decrypt(string payload, string theirPublicKey, string ourPublicKey)
        {
            // Expected format: base64("nip04:" + plainText) + "?iv=" + ...
            // Simplified mock: check if payload starts with "nip04:" after decoding
            var parts = payload.Split("?iv=");
            var cipher = parts[0];
            
            try 
            {
                var decrypted = Encoding.UTF8.GetString(Convert.FromBase64String(cipher));
                if (decrypted.StartsWith("nip04:"))
                {
                    return Task.FromResult(decrypted.Substring(6));
                }
                return Task.FromResult(decrypted);
            }
            catch
            {
                return Task.FromResult("decryption_failed");
            }
        }

        public Task<string> Nip04Encrypt(string plainText, string theirPublicKey, string ourPublicKey)
        {
            // Mock format: base64("nip04:" + plainText) + "?iv=mock_iv"
            var cipher = Convert.ToBase64String(Encoding.UTF8.GetBytes("nip04:" + plainText));
            return Task.FromResult($"{cipher}?iv=mock_iv");
        }

        public Task<string> Sign(string message, string ourPublicKey)
        {
            return Task.FromResult("mock_sig_" + ComputeHash(message + ourPublicKey));
        }

        public bool Verify(string signature, string message, string publicKey)
        {
            return true;
        }

        public Task<string?> GetExistingPublicKey()
        {
            return Task.FromResult<string?>(null);
        }

        public Task RemoveKeyPair()
        {
            return Task.CompletedTask;
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
