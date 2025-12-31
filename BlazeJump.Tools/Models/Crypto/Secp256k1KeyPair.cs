namespace BlazeJump.Tools.Models.Crypto
{
	/// <summary>
	/// Represents a Secp256k1 cryptographic key pair (private and public keys) using hex-encoded strings.
	/// This is a simple representation suitable for storage and serialization.
	/// </summary>
	public class Secp256k1KeyPair
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Secp256k1KeyPair"/> class.
		/// </summary>
		/// <param name="privateKey">The hex-encoded private key (64 characters).</param>
		/// <param name="publicKey">The hex-encoded public key (66 characters with prefix, 64 without).</param>
		public Secp256k1KeyPair(string privateKey, string publicKey) { 
			PrivateKey = privateKey;
			PublicKey = publicKey;
		}
		
		/// <summary>
		/// Gets the hex-encoded private key.
		/// </summary>
		public string PrivateKey { get; private set; }
		
		/// <summary>
		/// Gets the hex-encoded public key.
		/// </summary>
		public string PublicKey { get; private set; }
	}
}