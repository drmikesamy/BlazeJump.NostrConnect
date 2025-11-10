using BlazeJump.Tools.Models.NostrConnect;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Provides Nostr Connect (NIP-46) protocol services for remote signing.
	/// </summary>
	public interface INostrConnectService
	{
		/// <summary>
		/// Occurs when a signing request is received and needs approval.
		/// </summary>
		event EventHandler<NostrConnectRequestEventArgs>? SigningRequestReceived;

		/// <summary>
		/// Handles an incoming Nostr Connect request.
		/// </summary>
		/// <param name="request">The decrypted request.</param>
		/// <param name="requesterPubkey">The public key of the requester.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task HandleRequest(NostrConnectRequest request, string requesterPubkey);

		/// <summary>
		/// Approves and executes a signing request.
		/// </summary>
		/// <param name="requestId">The request ID to approve.</param>
		/// <param name="approved">Whether the request is approved.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task RespondToSigningRequest(string requestId, bool approved);

		/// <summary>
		/// Generates a connection URI for QR code display (bunker:// protocol).
		/// </summary>
		/// <param name="relay">The relay URL to use for communication.</param>
		/// <returns>The bunker:// URI string.</returns>
		Task<string> GenerateConnectionUri(string relay);
	}

	/// <summary>
	/// Event arguments for Nostr Connect signing requests.
	/// </summary>
	public class NostrConnectRequestEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the request ID.
		/// </summary>
		public string RequestId { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the method being requested.
		/// </summary>
		public string Method { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the request parameters.
		/// </summary>
		public List<string> Params { get; set; } = new();

		/// <summary>
		/// Gets or sets the requester's public key.
		/// </summary>
		public string RequesterPubkey { get; set; } = string.Empty;
	}
}
