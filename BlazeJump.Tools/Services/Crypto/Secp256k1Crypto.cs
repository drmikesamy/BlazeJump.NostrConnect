using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;

namespace BlazeJump.Tools.Services.Crypto
{
	/// <summary>
	/// Pure C# implementation of secp256k1 operations using BouncyCastle.
	/// Provides key generation, ECDH, and Schnorr signatures (BIP-340).
	/// </summary>
	public static class Secp256k1Crypto
	{
		private static readonly X9ECParameters _curve = ECNamedCurveTable.GetByName("secp256k1");
		private static readonly ECDomainParameters _domainParams = new ECDomainParameters(
			_curve.Curve,
			_curve.G,
			_curve.N,
			_curve.H
		);
		private static readonly SecureRandom _secureRandom = new SecureRandom();

		#region Key Generation

		/// <summary>
		/// Generates a new random 32-byte private key.
		/// </summary>
		public static byte[] GeneratePrivateKey()
		{
			byte[] privateKey = new byte[32];
			
			do
			{
				_secureRandom.NextBytes(privateKey);
			}
			while (!IsValidPrivateKey(privateKey));
			
			return privateKey;
		}

		/// <summary>
		/// Derives the public key from a private key.
		/// Returns 65-byte uncompressed format (0x04 + x + y).
		/// </summary>
		public static byte[] GetPublicKey(byte[] privateKeyBytes, bool compressed = false)
		{
			if (!IsValidPrivateKey(privateKeyBytes))
				throw new ArgumentException("Invalid private key", nameof(privateKeyBytes));

			var d = new BigInteger(1, privateKeyBytes);
			var q = _curve.G.Multiply(d).Normalize();
			return q.GetEncoded(compressed);
		}

		/// <summary>
		/// Gets the x-only public key (32 bytes, Schnorr/Taproot format).
		/// </summary>
		public static byte[] GetXOnlyPublicKey(byte[] privateKeyBytes)
		{
			var pubKey = GetPublicKey(privateKeyBytes, compressed: false);
			
			// Extract x-coordinate (bytes 1-32, skipping the 0x04 prefix)
			byte[] xOnly = new byte[32];
			Array.Copy(pubKey, 1, xOnly, 0, 32);
			return xOnly;
		}

		#endregion

		#region ECDH

		/// <summary>
		/// Performs ECDH to compute a shared secret.
		/// </summary>
		/// <param name="privateKeyBytes">32-byte private key</param>
		/// <param name="publicKeyBytes">33-byte compressed or 65-byte uncompressed public key</param>
		/// <returns>33-byte compressed shared secret point</returns>
		public static byte[] ComputeSharedSecret(byte[] privateKeyBytes, byte[] publicKeyBytes)
		{
			if (privateKeyBytes == null || privateKeyBytes.Length != 32)
				throw new ArgumentException("Private key must be 32 bytes", nameof(privateKeyBytes));

			if (publicKeyBytes == null || (publicKeyBytes.Length != 33 && publicKeyBytes.Length != 65))
				throw new ArgumentException("Public key must be 33 (compressed) or 65 (uncompressed) bytes", nameof(publicKeyBytes));

			// Parse private key as BigInteger
			var d = new BigInteger(1, privateKeyBytes);
			
			// Parse public key as EC point
			ECPoint publicKeyPoint = _curve.Curve.DecodePoint(publicKeyBytes);
			
			// Perform scalar multiplication: sharedSecret = privateKey * publicKeyPoint
			ECPoint sharedPoint = publicKeyPoint.Multiply(d).Normalize();
			
			// Return compressed encoding (33 bytes: 0x02/0x03 + 32-byte x-coordinate)
			return sharedPoint.GetEncoded(compressed: true);
		}

		/// <summary>
		/// Performs ECDH and returns the x-coordinate only (32 bytes).
		/// This is the format used by NIP-44 and most Nostr implementations.
		/// </summary>
		/// <param name="privateKeyBytes">32-byte private key</param>
		/// <param name="publicKeyBytes">33-byte compressed or 65-byte uncompressed public key</param>
		/// <returns>32-byte x-coordinate of shared secret point</returns>
		public static byte[] ComputeSharedSecretXOnly(byte[] privateKeyBytes, byte[] publicKeyBytes)
		{
			var compressed = ComputeSharedSecret(privateKeyBytes, publicKeyBytes);
			
			// Extract x-coordinate (skip first byte which is 0x02 or 0x03)
			var xCoordinate = new byte[32];
			Array.Copy(compressed, 1, xCoordinate, 0, 32);
			return xCoordinate;
		}

		#endregion

		#region Schnorr Signatures (BIP-340)

		/// <summary>
		/// Signs a message hash using BIP-340 Schnorr signatures.
		/// </summary>
		/// <param name="messageHash">32-byte message hash</param>
		/// <param name="privateKeyBytes">32-byte private key</param>
		/// <returns>64-byte Schnorr signature</returns>
		public static byte[] SignSchnorr(byte[] messageHash, byte[] privateKeyBytes)
		{
			if (messageHash == null || messageHash.Length != 32)
				throw new ArgumentException("Message hash must be 32 bytes", nameof(messageHash));
			
			if (!IsValidPrivateKey(privateKeyBytes))
				throw new ArgumentException("Invalid private key", nameof(privateKeyBytes));

			var d = new BigInteger(1, privateKeyBytes);
			var P = _curve.G.Multiply(d).Normalize();
			
			// Get x-only public key
			var Px = P.AffineXCoord.ToBigInteger();
			
			// BIP-340: if P.y is odd, negate d
			var Py = P.AffineYCoord.ToBigInteger();
			if (!Py.TestBit(0)) // if y is even
			{
				// Use d as-is
			}
			else // y is odd
			{
				d = _curve.N.Subtract(d);
			}
			
			// Generate nonce using tagged hash
			var sha256 = new Sha256Digest();
			var tag = System.Text.Encoding.UTF8.GetBytes("BIP0340/aux");
			var tagHash = new byte[32];
			sha256.BlockUpdate(tag, 0, tag.Length);
			sha256.DoFinal(tagHash, 0);
			
			// Create auxiliary random data
			byte[] auxRand = new byte[32];
			_secureRandom.NextBytes(auxRand);
			
			// t = d XOR hash(tag || tag || auxRand)
			var tData = new byte[64 + 32];
			Array.Copy(tagHash, 0, tData, 0, 32);
			Array.Copy(tagHash, 0, tData, 32, 32);
			Array.Copy(auxRand, 0, tData, 64, 32);
			sha256.Reset();
			sha256.BlockUpdate(tData, 0, tData.Length);
			var tHash = new byte[32];
			sha256.DoFinal(tHash, 0);
			
			var t = new byte[32];
			for (int i = 0; i < 32; i++)
			{
				t[i] = (byte)(privateKeyBytes[i] ^ tHash[i]);
			}
			
			// Generate nonce k = hash("BIP0340/nonce", t, Px, m) mod n
			var nonceTag = System.Text.Encoding.UTF8.GetBytes("BIP0340/nonce");
			sha256.Reset();
			sha256.BlockUpdate(nonceTag, 0, nonceTag.Length);
			var nonceTagHash = new byte[32];
			sha256.DoFinal(nonceTagHash, 0);
			
			var nonceData = new byte[64 + 32 + 32 + 32];
			Array.Copy(nonceTagHash, 0, nonceData, 0, 32);
			Array.Copy(nonceTagHash, 0, nonceData, 32, 32);
			Array.Copy(t, 0, nonceData, 64, 32);
			Array.Copy(Px.ToByteArrayUnsigned().PadLeft(32), 0, nonceData, 96, 32);
			Array.Copy(messageHash, 0, nonceData, 128, 32);
			
			sha256.Reset();
			sha256.BlockUpdate(nonceData, 0, nonceData.Length);
			var kBytes = new byte[32];
			sha256.DoFinal(kBytes, 0);
			var k = new BigInteger(1, kBytes).Mod(_curve.N);
			
			if (k.SignValue == 0)
				throw new Exception("Invalid nonce generated");
			
			// R = k*G
			var R = _curve.G.Multiply(k).Normalize();
			var Rx = R.AffineXCoord.ToBigInteger();
			var Ry = R.AffineYCoord.ToBigInteger();
			
			// If Ry is odd, negate k
			if (Ry.TestBit(0))
			{
				k = _curve.N.Subtract(k);
			}
			
			// e = hash("BIP0340/challenge", Rx, Px, m) mod n
			var challengeTag = System.Text.Encoding.UTF8.GetBytes("BIP0340/challenge");
			sha256.Reset();
			sha256.BlockUpdate(challengeTag, 0, challengeTag.Length);
			var challengeTagHash = new byte[32];
			sha256.DoFinal(challengeTagHash, 0);
			
			var challengeData = new byte[64 + 32 + 32 + 32];
			Array.Copy(challengeTagHash, 0, challengeData, 0, 32);
			Array.Copy(challengeTagHash, 0, challengeData, 32, 32);
			Array.Copy(Rx.ToByteArrayUnsigned().PadLeft(32), 0, challengeData, 64, 32);
			Array.Copy(Px.ToByteArrayUnsigned().PadLeft(32), 0, challengeData, 96, 32);
			Array.Copy(messageHash, 0, challengeData, 128, 32);
			
			sha256.Reset();
			sha256.BlockUpdate(challengeData, 0, challengeData.Length);
			var eBytes = new byte[32];
			sha256.DoFinal(eBytes, 0);
			var e = new BigInteger(1, eBytes).Mod(_curve.N);
			
			// s = (k + e*d) mod n
			var s = k.Add(e.Multiply(d)).Mod(_curve.N);
			
			// Signature is (Rx || s)
			var signature = new byte[64];
			Array.Copy(Rx.ToByteArrayUnsigned().PadLeft(32), 0, signature, 0, 32);
			Array.Copy(s.ToByteArrayUnsigned().PadLeft(32), 0, signature, 32, 32);
			
			return signature;
		}

		/// <summary>
		/// Verifies a BIP-340 Schnorr signature.
		/// </summary>
		/// <param name="messageHash">32-byte message hash</param>
		/// <param name="signature">64-byte signature</param>
		/// <param name="publicKeyXOnly">32-byte x-only public key</param>
		/// <returns>True if signature is valid</returns>
		public static bool VerifySchnorr(byte[] messageHash, byte[] signature, byte[] publicKeyXOnly)
		{
			if (messageHash == null || messageHash.Length != 32)
				return false;
			
			if (signature == null || signature.Length != 64)
				return false;
			
			if (publicKeyXOnly == null || publicKeyXOnly.Length != 32)
				return false;

			try
			{
				// Parse signature (r, s)
				var r = new BigInteger(1, signature, 0, 32);
				var s = new BigInteger(1, signature, 32, 32);
				
				// Check r and s are in valid range
				if (r.CompareTo(_curve.N) >= 0 || s.CompareTo(_curve.N) >= 0)
					return false;
				
				// Reconstruct public key point from x-only coordinate (use even y)
				var Px = new BigInteger(1, publicKeyXOnly);
				var P = GetPointFromX(Px, false); // BIP-340 uses even y
				
				if (P is null)
					return false;
				
				// e = hash("BIP0340/challenge", r, Px, m) mod n
				var sha256 = new Sha256Digest();
				var challengeTag = System.Text.Encoding.UTF8.GetBytes("BIP0340/challenge");
				sha256.BlockUpdate(challengeTag, 0, challengeTag.Length);
				var challengeTagHash = new byte[32];
				sha256.DoFinal(challengeTagHash, 0);
				
				var challengeData = new byte[64 + 32 + 32 + 32];
				Array.Copy(challengeTagHash, 0, challengeData, 0, 32);
				Array.Copy(challengeTagHash, 0, challengeData, 32, 32);
				Array.Copy(r.ToByteArrayUnsigned().PadLeft(32), 0, challengeData, 64, 32);
				Array.Copy(publicKeyXOnly, 0, challengeData, 96, 32);
				Array.Copy(messageHash, 0, challengeData, 128, 32);
				
				sha256.Reset();
				sha256.BlockUpdate(challengeData, 0, challengeData.Length);
				var eBytes = new byte[32];
				sha256.DoFinal(eBytes, 0);
				var e = new BigInteger(1, eBytes).Mod(_curve.N);
				
				// Verify: s*G == R + e*P
				var sG = _curve.G.Multiply(s).Normalize();
				var eP = P.Multiply(e).Normalize();
				var R = GetPointFromX(r, false);
				
				if (R is null)
					return false;
				
				var RplusEP = R.Add(eP).Normalize();
				
				return sG.Equals(RplusEP);
			}
			catch
			{
				return false;
			}
		}

		private static ECPoint? GetPointFromX(BigInteger x, bool oddY)
		{
			try
			{
				// y² = x³ + 7 (secp256k1 curve equation)
				var ySquared = x.ModPow(new BigInteger("3"), _curve.Curve.Field.Characteristic)
					.Add(new BigInteger("7"))
					.Mod(_curve.Curve.Field.Characteristic);
				
				// Calculate y = ySquared^((p+1)/4) mod p (works because p ≡ 3 (mod 4))
				var p = _curve.Curve.Field.Characteristic;
				var exponent = p.Add(BigInteger.One).Divide(new BigInteger("4"));
				var y = ySquared.ModPow(exponent, p);
				
				// Verify it's a valid square root
				if (!y.ModPow(new BigInteger("2"), p).Equals(ySquared))
					return null;
				
				// Choose correct y based on parity
				if (y.TestBit(0) != oddY)
				{
					y = p.Subtract(y);
				}
				
				return _curve.Curve.CreatePoint(x, y);
			}
			catch
			{
				return null;
			}
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates that a private key is within the valid range for secp256k1.
		/// </summary>
		public static bool IsValidPrivateKey(byte[] privateKeyBytes)
		{
			if (privateKeyBytes == null || privateKeyBytes.Length != 32)
				return false;

			var d = new BigInteger(1, privateKeyBytes);
			return d.CompareTo(BigInteger.One) >= 0 && d.CompareTo(_curve.N) < 0;
		}

		/// <summary>
		/// Validates that a public key is a valid point on the secp256k1 curve.
		/// </summary>
		public static bool IsValidPublicKey(byte[] publicKeyBytes)
		{
			if (publicKeyBytes == null || (publicKeyBytes.Length != 33 && publicKeyBytes.Length != 65))
				return false;

				try
			{
				ECPoint point = _curve.Curve.DecodePoint(publicKeyBytes);
				return point.IsValid();
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}

	// Extension method for padding
	internal static class BigIntegerExtensions
	{
		public static byte[] PadLeft(this byte[] bytes, int length)
		{
			if (bytes.Length >= length)
				return bytes;
			
			var padded = new byte[length];
			Array.Copy(bytes, 0, padded, length - bytes.Length, bytes.Length);
			return padded;
		}
	}
}

