using Newtonsoft.Json;

namespace BlazeJump.Tools.Models.NostrConnect
{
	/// <summary>
	/// Represents a Nostr Connect protocol request message.
	/// </summary>
	public class NostrConnectRequest
	{
		/// <summary>
		/// Gets or sets the request ID.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the method name being requested.
		/// </summary>
		[JsonProperty("method")]
		public string Method { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the list of parameters for the method.
		/// </summary>
		[JsonProperty("params")]
		public List<string> Params { get; set; } = new();
	}
}
