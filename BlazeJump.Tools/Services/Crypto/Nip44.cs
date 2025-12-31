using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;

namespace BlazeJump.Tools.Services.Crypto
{
    /// <summary>
    /// Implements NIP-44 v2 encryption/decryption for Nostr protocol.
    /// Full specification with padding, HKDF key derivation, and HMAC-SHA256 authentication.
    /// </summary>
    public static class Nip44
    {
        private const int NonceSize = 32; // NIP-44 v2 uses 32-byte nonce
        private const int MacSize = 32;   // HMAC-SHA256 produces 32 bytes
        private const int ConversationKeySize = 32;
        private const int MinPlaintextSize = 1;
        private const int MaxPlaintextSize = 65535;
        private const byte Version = 2; // NIP-44 version
        private static readonly SecureRandom _secureRandom = new SecureRandom();

        /// <summary>
        /// Encrypts a message using NIP-44 v2 encryption.
        /// </summary>
        /// <param name="plaintext">The message to encrypt.</param>
        /// <param name="conversationKey">The 32-byte conversation key derived from ECDH.</param>
        /// <returns>Base64-encoded ciphertext with format: version || nonce || ciphertext || mac</returns>
        public static string Encrypt(string plaintext, byte[] conversationKey)
        {
            if (conversationKey.Length != ConversationKeySize)
                throw new ArgumentException($"Conversation key must be {ConversationKeySize} bytes", nameof(conversationKey));

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            if (plaintextBytes.Length < MinPlaintextSize || plaintextBytes.Length > MaxPlaintextSize)
                throw new ArgumentException($"Plaintext length must be between {MinPlaintextSize} and {MaxPlaintextSize} bytes");

            // Pad the plaintext
            var padded = Pad(plaintextBytes);

            // Generate random 32-byte nonce
            var nonce = new byte[NonceSize];
            _secureRandom.NextBytes(nonce);

            // Derive message keys from conversation key and nonce
            var (chachaKey, chachaNonce, hmacKey) = DeriveMessageKeys(conversationKey, nonce);

            // Encrypt with ChaCha20 (not AEAD, just stream cipher)
            var ciphertext = ChaCha20Encrypt(chachaKey, chachaNonce, padded);

            // Calculate HMAC-SHA256 with AAD
            var mac = CalculateHmacAad(hmacKey, ciphertext, nonce);

            // Prepare output: version (1) + nonce (32) + ciphertext + mac (32)
            var output = new byte[1 + NonceSize + ciphertext.Length + MacSize];
            output[0] = Version;
            Array.Copy(nonce, 0, output, 1, NonceSize);
            Array.Copy(ciphertext, 0, output, 1 + NonceSize, ciphertext.Length);
            Array.Copy(mac, 0, output, 1 + NonceSize + ciphertext.Length, MacSize);

            var result = Convert.ToBase64String(output);
            Console.WriteLine($"[NIP-44 C#] Encrypt: version={Version}, nonceLen={nonce.Length}, ciphertextLen={ciphertext.Length}, macLen={mac.Length}, totalLen={output.Length}");
            Console.WriteLine($"[NIP-44 C#] Nonce (hex): {BitConverter.ToString(nonce).Replace("-", "").ToLower()}");
            Console.WriteLine($"[NIP-44 C#] MAC (hex): {BitConverter.ToString(mac).Replace("-", "").ToLower()}");
            return result;
        }

        /// <summary>
        /// Decrypts a NIP-44 v2 encrypted message.
        /// </summary>
        /// <param name="base64Payload">Base64-encoded payload with format: version || nonce || ciphertext || mac</param>
        /// <param name="conversationKey">The 32-byte conversation key derived from ECDH.</param>
        /// <returns>Decrypted plaintext.</returns>
        public static string Decrypt(string base64Payload, byte[] conversationKey)
        {
            if (conversationKey.Length != ConversationKeySize)
                throw new ArgumentException($"Conversation key must be {ConversationKeySize} bytes", nameof(conversationKey));

            var payload = Convert.FromBase64String(base64Payload);

            // Validate minimum size: version (1) + nonce (32) + mac (32) = 65 bytes minimum
            if (payload.Length < 1 + NonceSize + MacSize)
                throw new ArgumentException("Payload too short", nameof(base64Payload));

            // Check version
            var version = payload[0];
            if (version != Version)
                throw new ArgumentException($"Unsupported version: {version}. Expected {Version}", nameof(base64Payload));

            // Extract components
            var nonce = new byte[NonceSize];
            Array.Copy(payload, 1, nonce, 0, NonceSize);

            var ciphertextLength = payload.Length - 1 - NonceSize - MacSize;
            var ciphertext = new byte[ciphertextLength];
            Array.Copy(payload, 1 + NonceSize, ciphertext, 0, ciphertextLength);

            var mac = new byte[MacSize];
            Array.Copy(payload, 1 + NonceSize + ciphertextLength, mac, 0, MacSize);

            // Derive message keys from conversation key and nonce
            var (chachaKey, chachaNonce, hmacKey) = DeriveMessageKeys(conversationKey, nonce);

            // Verify HMAC
            var calculatedMac = CalculateHmacAad(hmacKey, ciphertext, nonce);
            if (!ConstantTimeEqual(calculatedMac, mac))
                throw new Exception("Invalid MAC - message authentication failed");

            // Decrypt with ChaCha20
            var paddedPlaintext = ChaCha20Encrypt(chachaKey, chachaNonce, ciphertext);

            // Remove padding
            var plaintextBytes = Unpad(paddedPlaintext);

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        /// <summary>
        /// Pads plaintext according to NIP-44 v2 padding scheme.
        /// </summary>
        private static byte[] Pad(byte[] unpadded)
        {
            int unpaddedLen = unpadded.Length;
            if (unpaddedLen < MinPlaintextSize || unpaddedLen > MaxPlaintextSize)
                throw new ArgumentException($"Invalid plaintext length: {unpaddedLen}");

            int paddedLen = CalculatePaddedLength(unpaddedLen);
            var padded = new byte[2 + paddedLen];

            // Write length prefix (big-endian)
            padded[0] = (byte)(unpaddedLen >> 8);
            padded[1] = (byte)(unpaddedLen & 0xFF);

            // Copy unpadded data
            Array.Copy(unpadded, 0, padded, 2, unpaddedLen);

            // Remaining bytes are already zero (padding)
            return padded;
        }

        /// <summary>
        /// Removes padding from plaintext according to NIP-44 v2 padding scheme.
        /// </summary>
        private static byte[] Unpad(byte[] padded)
        {
            if (padded.Length < 2)
                throw new ArgumentException("Invalid padded data");

            // Read length prefix (big-endian)
            int unpaddedLen = (padded[0] << 8) | padded[1];

            if (unpaddedLen < MinPlaintextSize || unpaddedLen > MaxPlaintextSize)
                throw new ArgumentException($"Invalid unpadded length: {unpaddedLen}");

            if (padded.Length != 2 + CalculatePaddedLength(unpaddedLen))
                throw new ArgumentException("Invalid padding");

            var unpadded = new byte[unpaddedLen];
            Array.Copy(padded, 2, unpadded, 0, unpaddedLen);

            return unpadded;
        }

        /// <summary>
        /// Calculates the padded length according to NIP-44 v2 padding scheme.
        /// </summary>
        private static int CalculatePaddedLength(int unpaddedLen)
        {
            if (unpaddedLen <= 32) return 32;

            int nextPower = 1;
            while (nextPower < unpaddedLen)
                nextPower *= 2;

            int chunk = nextPower <= 256 ? 32 : nextPower / 8;
            return chunk * ((unpaddedLen - 1) / chunk + 1);
        }

        /// <summary>
        /// Derives message-specific keys from conversation key and nonce using HKDF-Expand.
        /// </summary>
        private static (byte[] chachaKey, byte[] chachaNonce, byte[] hmacKey) DeriveMessageKeys(byte[] conversationKey, byte[] nonce)
        {
            // HKDF-Expand to derive 76 bytes: chacha_key (32) + chacha_nonce (12) + hmac_key (32)
            var output = new byte[76];
            HkdfExpand(conversationKey, output, nonce);

            var chachaKey = new byte[32];
            var chachaNonce = new byte[12];
            var hmacKey = new byte[32];

            Array.Copy(output, 0, chachaKey, 0, 32);
            Array.Copy(output, 32, chachaNonce, 0, 12);
            Array.Copy(output, 44, hmacKey, 0, 32);

            return (chachaKey, chachaNonce, hmacKey);
        }

        /// <summary>
        /// HKDF-Expand implementation using BouncyCastle.
        /// </summary>
        private static void HkdfExpand(byte[] prk, byte[] output, byte[] info)
        {
            int hashLen = 32; // SHA256
            int n = (output.Length + hashLen - 1) / hashLen;

            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(prk));
            var previousBlock = Array.Empty<byte>();

            for (int i = 1; i <= n; i++)
            {
                hmac.BlockUpdate(previousBlock, 0, previousBlock.Length);
                hmac.BlockUpdate(info, 0, info.Length);
                hmac.Update((byte)i);

                previousBlock = new byte[hashLen];
                hmac.DoFinal(previousBlock, 0);
                
                int offset = (i - 1) * hashLen;
                int copyLen = Math.Min(hashLen, output.Length - offset);
                Array.Copy(previousBlock, 0, output, offset, copyLen);
                
                hmac.Reset();
            }
        }

        /// <summary>
        /// Encrypts data using ChaCha20 stream cipher.
        /// </summary>
        private static byte[] ChaCha20Encrypt(byte[] key, byte[] nonce, byte[] data)
        {
            if (key.Length != 32)
                throw new ArgumentException("ChaCha20 key must be 32 bytes", nameof(key));
            if (nonce.Length != 12)
                throw new ArgumentException("ChaCha20 nonce must be 12 bytes", nameof(nonce));

            var engine = new ChaCha7539Engine();
            var parameters = new ParametersWithIV(new KeyParameter(key), nonce);
            engine.Init(true, parameters);

            var result = new byte[data.Length];
            engine.ProcessBytes(data, 0, data.Length, result, 0);

            return result;
        }

        /// <summary>
        /// Calculates HMAC-SHA256 with AAD (Additional Authenticated Data).
        /// </summary>
        private static byte[] CalculateHmacAad(byte[] key, byte[] message, byte[] aad)
        {
            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));

            // HMAC(key, aad || message)
            hmac.BlockUpdate(aad, 0, aad.Length);
            hmac.BlockUpdate(message, 0, message.Length);

            var result = new byte[32];
            hmac.DoFinal(result, 0);

            return result;
        }

        /// <summary>
        /// Constant-time equality comparison to prevent timing attacks.
        /// </summary>
        private static bool ConstantTimeEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
                result |= a[i] ^ b[i];

            return result == 0;
        }

        /// <summary>
        /// Derives a conversation key from ECDH shared secret using HKDF.
        /// This implements the NIP-44 key derivation.
        /// </summary>
        /// <param name="sharedSecret">The 32-byte shared secret from ECDH (x-coordinate only).</param>
        /// <returns>The 32-byte conversation key.</returns>
        public static byte[] DeriveConversationKey(byte[] sharedSecret)
        {
            if (sharedSecret.Length != 32)
                throw new ArgumentException("Shared secret must be 32 bytes", nameof(sharedSecret));

            // NIP-44 uses HKDF with salt = "nip44-v2" and no info parameter
            var salt = Encoding.UTF8.GetBytes("nip44-v2");
            var conversationKey = new byte[ConversationKeySize];

            // HKDF-Extract: PRK = HMAC-SHA256(salt, IKM)
            var hmacExtract = new HMac(new Sha256Digest());
            hmacExtract.Init(new KeyParameter(salt));
            hmacExtract.BlockUpdate(sharedSecret, 0, sharedSecret.Length);
            var prk = new byte[32];
            hmacExtract.DoFinal(prk, 0);

            // HKDF-Expand: OKM = HKDF-Expand(PRK, info, L)
            HkdfExpand(prk, conversationKey, Array.Empty<byte>());

            return conversationKey;
        }
    }
}
