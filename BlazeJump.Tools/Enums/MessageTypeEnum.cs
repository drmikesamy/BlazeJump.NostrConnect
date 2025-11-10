namespace BlazeJump.Tools.Enums
{
	/// <summary>
	/// Message types used in Nostr relay communication protocol.
	/// </summary>
	public enum MessageTypeEnum
	{
		/// <summary>
		/// Request message to query events from a relay.
		/// </summary>
		Req = 0,
		
		/// <summary>
		/// Event message containing a Nostr event.
		/// </summary>
		Event = 1,
		
		/// <summary>
		/// Notice message from a relay.
		/// </summary>
		Notice = 2,
		
		/// <summary>
		/// Close message to end a subscription.
		/// </summary>
		Close = 3,
		
		/// <summary>
		/// OK message indicating acceptance or rejection of an event.
		/// </summary>
		Ok = 4,
		
		/// <summary>
		/// End of stored events message.
		/// </summary>
		Eose = 5,
		
		/// <summary>
		/// Count message for event counting queries.
		/// </summary>
		Count = 6
	}
}
