namespace BlazeJump.Tools.Enums
{
	/// <summary>
	/// TLV (Type-Length-Value) types used in Bech32 encoded data.
	/// </summary>
	public enum TLVTypeEnum
	{
		/// <summary>
		/// Special purpose TLV type (type 0).
		/// </summary>
		Special = 0,
		
		/// <summary>
		/// Relay URL TLV type (type 1).
		/// </summary>
		Relay = 1,
		
		/// <summary>
		/// Author public key TLV type (type 2).
		/// </summary>
		Author = 2,
		
		/// <summary>
		/// Event kind TLV type (type 3).
		/// </summary>
		Kind = 3
	}
}