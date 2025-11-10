namespace BlazeJump.Tools.Enums
{
	/// <summary>
	/// Bech32 encoding prefix types used in Nostr protocol.
	/// </summary>
	public enum Bech32PrefixEnum
	{
		/// <summary>
		/// Profile identifier with metadata.
		/// </summary>
		nprofile = 0,
		
		/// <summary>
		/// Event identifier with metadata.
		/// </summary>
		nevent = 1,
		
		/// <summary>
		/// Public key identifier.
		/// </summary>
		npub = 2,
		
		/// <summary>
		/// Secret/private key identifier.
		/// </summary>
		nsec = 3,
		
		/// <summary>
		/// Address identifier.
		/// </summary>
		naddr = 4
	}
}
