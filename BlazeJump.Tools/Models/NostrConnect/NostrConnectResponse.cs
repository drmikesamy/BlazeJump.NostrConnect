using Newtonsoft.Json;

namespace BlazeJump.Tools.Models.NostrConnect
{
	/// <summary>
	/// Represents a Nostr Connect protocol response message.
	/// </summary>
	public class NostrConnectResponse
	{
		/// <summary>
		/// Gets or sets the response ID (matches the request ID).
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the result data (on success).
		/// </summary>
		[JsonProperty("result")]
		public string Result { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the error message (if the request failed).
		/// </summary>
		[JsonProperty("error")]
		public string? Error { get; set; }
	}
}
