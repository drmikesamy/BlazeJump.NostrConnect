using BlazeJump.Tools.Enums;
using BlazeJump.Tools.JsonConverters;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a Nostr relay protocol message that can be sent or received.
	/// </summary>
	[JsonConverter(typeof(MessageConverter))]
	public class NMessage
	{
		/// <summary>
		/// Gets or sets the type of relay message.
		/// </summary>
		public MessageTypeEnum MessageType { get; set; }
		
		/// <summary>
		/// Gets or sets the subscription ID (for REQ, EVENT, EOSE, CLOSE messages).
		/// </summary>
		public string? SubscriptionId { get; set; }
		
		/// <summary>
		/// Gets or sets the event data (for EVENT messages).
		/// </summary>
		public NEvent? Event { get; set; }
		
		/// <summary>
		/// Gets or sets the filter data (for REQ messages).
		/// </summary>
		public Filter? Filter { get; set; }
		
		/// <summary>
		/// Gets or sets the notice message text (for NOTICE messages).
		/// </summary>
		public string? NoticeMessage { get; set; }
		
		/// <summary>
		/// Gets or sets whether the operation was successful (for OK messages).
		/// </summary>
		public bool? Success { get; set; }
		
		/// <summary>
		/// Gets or sets the event ID being acknowledged (for OK messages).
		/// </summary>
		public string? NEventId { get; set; }
	}
}