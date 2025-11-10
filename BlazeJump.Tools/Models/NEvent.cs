using AutoMapper;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a Nostr event with all standard fields and computed properties.
	/// </summary>
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class NEvent
    {
		/// <summary>
		/// Gets or sets the event ID (32-byte hex-encoded hash).
		/// </summary>
		[JsonProperty(Order = 1)]
		public string Id { get; set; } = string.Empty;
		
		/// <summary>
		/// Gets or sets the author's public key (32-byte hex-encoded).
		/// </summary>
		[JsonProperty(Order = 2)]
		public string? Pubkey { get; set; }
		
		/// <summary>
		/// Gets or sets the creation timestamp (Unix time in seconds).
		/// </summary>
		[JsonProperty(Order = 3)]
		public long Created_At { get; set; }
		
		/// <summary>
		/// Gets or sets the event kind.
		/// </summary>
		[JsonProperty(Order = 4)]
		public KindEnum? Kind { get; set; }
		
		/// <summary>
		/// Gets or sets the list of tags attached to this event.
		/// </summary>
		[JsonProperty(Order = 5)]
		public List<EventTag>? Tags { get; set; } = new List<EventTag>();
		
		/// <summary>
		/// Gets or sets the event content (arbitrary string).
		/// </summary>
		[JsonProperty(Order = 6)]
		public string? Content { get; set; }
		
		/// <summary>
		/// Gets or sets the event signature (64-byte hex-encoded).
		/// </summary>
		[JsonProperty(Order = 7)]
		public string? Sig { get; set; }
		
		/// <summary>
		/// Gets or sets the user/author information (not serialized).
		/// </summary>
		[JsonIgnore]
        public User? User { get; set; }
		
		/// <summary>
		/// Gets the created timestamp as a local DateTime.
		/// </summary>
		[JsonIgnore]
		public DateTime CreatedAtDateTime => GeneralHelpers.UnixTimeStampToDateTime(Created_At).ToLocalTime();
        
		/// <summary>
		/// Gets the root event ID from tags (if this is a reply in a thread).
		/// </summary>
		[JsonIgnore]
		public string? RootId => Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "root")?.Value;

		/// <summary>
		/// Gets the parent event ID from tags (if this is a direct reply).
		/// </summary>
		[JsonIgnore]
		public string? ParentId => Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "reply")?.Value;        /// <summary>
        /// Gets or sets whether the event signature has been verified.
        /// </summary>
        [JsonIgnore]
        public bool Verified { get; set; } = false;
	}

}