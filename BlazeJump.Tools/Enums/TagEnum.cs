namespace BlazeJump.Tools.Enums
{
	/// <summary>
	/// Tag types used in Nostr events as defined in the protocol specification.
	/// </summary>
	public enum TagEnum
	{
		/// <summary>
		/// Event tag referencing another event.
		/// </summary>
		e = 0,
		
		/// <summary>
		/// Public key tag referencing a user.
		/// </summary>
		p = 1,
		
		/// <summary>
		/// Address tag for replaceable events.
		/// </summary>
		a = 2,
		
		/// <summary>
		/// Reference tag for URLs or other references.
		/// </summary>
		r = 3,
		
		/// <summary>
		/// Topic/hashtag tag.
		/// </summary>
		t = 4,
		
		/// <summary>
		/// Geohash tag for location.
		/// </summary>
		g = 5,
		
		/// <summary>
		/// Nonce tag for proof of work.
		/// </summary>
		nonce = 6,
		
		/// <summary>
		/// Subject tag for event subject/title.
		/// </summary>
		subject = 7,
		
		/// <summary>
		/// Identifier tag for replaceable events.
		/// </summary>
		d = 8,
		
		/// <summary>
		/// Expiration timestamp tag.
		/// </summary>
		expiration = 9,
		
		/// <summary>
		/// Quote tag for quoted events.
		/// </summary>
		q = 10,
		
		/// <summary>
		/// Image metadata tag.
		/// </summary>
		imeta =11,
		
		/// <summary>
		/// Proxy tag for external resources.
		/// </summary>
		proxy = 12
	}
}
