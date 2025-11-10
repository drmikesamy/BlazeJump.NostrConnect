using AutoMapper;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a Nostr event that can be signed (before computing the ID and signature).
	/// </summary>
	[JsonConverter(typeof(SignableNEventConverter))]
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class SignableNEvent
    {
		/// <summary>
		/// Gets the event ID placeholder (always 0 for signing purposes).
		/// </summary>
		[JsonProperty(Order = 1)]
		public int Id { get; } = 0;
		
		/// <summary>
		/// Gets or sets the author's public key.
		/// </summary>
		[JsonProperty(Order = 2)]
		public string? Pubkey { get; set; }
		
		/// <summary>
		/// Gets or sets the creation timestamp.
		/// </summary>
		[JsonProperty(Order = 3)]
		public long Created_At { get; set; }
		
		/// <summary>
		/// Gets or sets the event kind.
		/// </summary>
		[JsonProperty(Order = 4)]
		public KindEnum? Kind { get; set; }
		
		/// <summary>
		/// Gets or sets the list of tags.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<EventTag>? Tags { get; set; } = new List<EventTag>();
		
		/// <summary>
		/// Gets or sets the event content.
		/// </summary>
		[JsonProperty(Order = 6)]
		public string? Content { get; set; }
	}
}