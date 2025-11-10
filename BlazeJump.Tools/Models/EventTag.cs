using BlazeJump.Tools.Enums;
using BlazeJump.Tools.JsonConverters;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a tag in a Nostr event with a key and up to 4 values.
	/// </summary>
	[JsonConverter(typeof(TagConverter))]
	public class EventTag
	{
		/// <summary>
		/// Gets or sets the tag key/type.
		/// </summary>
		public TagEnum Key { get; set; }
		
		/// <summary>
		/// Gets or sets the first tag value.
		/// </summary>
		public string? Value { get; set; }
		
		/// <summary>
		/// Gets or sets the second tag value (optional).
		/// </summary>
		public string? Value2 { get; set; }
		
		/// <summary>
		/// Gets or sets the third tag value (optional).
		/// </summary>
		public string? Value3 { get; set; }
		
		/// <summary>
		/// Gets or sets the fourth tag value (optional).
		/// </summary>
		public string? Value4 { get; set; }
	}

}