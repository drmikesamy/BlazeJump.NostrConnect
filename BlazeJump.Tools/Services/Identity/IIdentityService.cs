using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Provides identity services for Nostr Connect authentication.
	/// </summary>
	public interface IIdentityService
	{
		/// <summary>
		/// Event raised when a session's state changes.
		/// </summary>
		public event EventHandler<NostrConnectSession>? NotifySessionStateChanged;
		
		/// <summary>
		/// Event raised when a ping response (pong) is received from a remote peer.
		/// </summary>
		public event EventHandler<NostrConnectResponse>? PingReceived;
		
		/// <summary>
		/// Listens for Nostr Connect messages on specified relays for a given public key
		/// </summary>
		/// <param name="relays">List of relay URLs to listen on</param>
		/// <param name="pubkey">Public key to filter messages for</param>
		Task ListenForNostrConnectMessages(List<string> relays, string pubkey);
		
		/// <summary>
		/// Sends a ping request to the specified session to test connectivity.
		/// </summary>
		/// <param name="session">The session to ping</param>
		Task SendPing(NostrConnectSession session);
		
		/// <summary>
		/// Sends a disconnect request to the specified session.
		/// </summary>
		/// <param name="session">The session to disconnect</param>
		Task SendDisconnect(NostrConnectSession session);
		
		/// <summary>
		/// Gets the dictionary of user profiles indexed by public key.
		/// </summary>
		Dictionary<string, UserProfile> UserProfiles { get; }
		
		/// <summary>
		/// Gets or sets the currently active user profile.
		/// </summary>
		UserProfile? ActiveUserProfile { get; set; }
		
		/// <summary>
		/// Creates a new user profile with an optional private key. If no private key is provided, a new one will be generated.
		/// </summary>
		/// <param name="privateKey">Optional private key in hex format. If null, a new key pair will be generated.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		Task CreateUserProfile(string? privateKey = null);
	}
}