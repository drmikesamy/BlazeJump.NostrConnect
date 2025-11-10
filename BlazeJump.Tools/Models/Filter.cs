using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BlazeJump.Tools.Models
{
	/// <summary>
	/// Represents a Nostr event filter for querying events from relays.
	/// </summary>
	public class Filter
    {
		/// <summary>
		/// Gets or sets a list of event IDs to filter by.
		/// </summary>
		[JsonProperty("ids", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? EventIds { get; set; }
		
		/// <summary>
		/// Gets or sets a list of author public keys to filter by.
		/// </summary>
		[JsonProperty("authors", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? Authors { get; set; }
		
		/// <summary>
		/// Gets or sets a list of event kinds to filter by.
		/// </summary>
		[JsonProperty("kinds", NullValueHandling = NullValueHandling.Ignore)] 
		public List<int>? Kinds { get; set; }
		
		/// <summary>
		/// Gets or sets a list of event IDs referenced in 'e' tags.
		/// </summary>
		[JsonProperty("#e", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? TaggedEventIds { get; set; }
		
		/// <summary>
		/// Gets or sets a list of public keys referenced in 'p' tags.
		/// </summary>
		[JsonProperty("#p", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? TaggedPublicKeys { get; set; }
		
		/// <summary>
		/// Gets or sets a list of keywords referenced in 't' tags.
		/// </summary>
		[JsonProperty("#t", NullValueHandling = NullValueHandling.Ignore)]
		public List<string>? TaggedKeywords { get; set; }
		
		/// <summary>
		/// Gets or sets the start timestamp for filtering events (Unix timestamp).
		/// </summary>
		[JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime? Since { get; set; }
		
		/// <summary>
		/// Gets or sets the end timestamp for filtering events (Unix timestamp).
		/// </summary>
		[JsonProperty("until", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime? Until { get; set; }
		
		/// <summary>
		/// Gets or sets the maximum number of events to return.
		/// </summary>
		[JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)] 
		public int? Limit { get; set; }
		
		/// <summary>
		/// Gets or sets a search query string for full-text search.
		/// </summary>
		[JsonProperty("search", NullValueHandling = NullValueHandling.Ignore)]
		public string? Search { get; set; }
	}
}