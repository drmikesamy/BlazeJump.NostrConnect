using System.Security.Cryptography;
using System.Text;
using BlazeJump.Tools.Services.Crypto;

namespace BlazeJump.Tools.Helpers
{
    public static class Nip04
    {
        public static string Encrypt(string plainText, byte[] sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (sharedSecret == null || sharedSecret.Length != 32)
                throw new ArgumentException("Invalid shared secret");

            using (var aes = Aes.Create())
            {
                aes.Key = sharedSecret;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // Generates a random 16-byte IV

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    string ivBase64 = Convert.ToBase64String(aes.IV);
                    string cipherBase64 = Convert.ToBase64String(encryptedBytes);

                    return $"{cipherBase64}?iv={ivBase64}";
                }
            }
        }

        public static string Decrypt(string payload, byte[] sharedSecret)
        {
            if (string.IsNullOrEmpty(payload))
                throw new ArgumentNullException(nameof(payload));
            if (sharedSecret == null || sharedSecret.Length != 32)
                throw new ArgumentException("Invalid shared secret");

            var parts = payload.Split("?iv=");
            if (parts.Length != 2)
                throw new ArgumentException("Invalid payload format. Expected 'ciphertext?iv=iv_base64'");

            byte[] cipherBytes = Convert.FromBase64String(parts[0]);
            byte[] ivBytes = Convert.FromBase64String(parts[1]);

            using (var aes = Aes.Create())
            {
                aes.Key = sharedSecret;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}
