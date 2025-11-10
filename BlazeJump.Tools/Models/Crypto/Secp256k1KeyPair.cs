using NBitcoin.Secp256k1;
namespace BlazeJump.Tools.Models.Crypto
{
	/// <summary>
	/// Represents a Secp256k1 cryptographic key pair (private and public keys).
	/// </summary>
	public class Secp256k1KeyPair
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Secp256k1KeyPair"/> class.
		/// </summary>
		/// <param name="privateKey">The private key.</param>
		/// <param name="publicKey">The public key.</param>
		public Secp256k1KeyPair(ECPrivKey privateKey, ECPubKey publicKey) { 
			PrivateKey = privateKey;
			PublicKey = publicKey;
		}
		
		/// <summary>
		/// Gets the private key.
		/// </summary>
		public ECPrivKey PrivateKey { get; private set; }
		
		/// <summary>
		/// Gets the public key.
		/// </summary>
		public ECPubKey PublicKey { get; private set; }
	}
}