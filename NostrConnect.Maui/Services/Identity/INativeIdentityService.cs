using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Services.Identity;


namespace NostrConnect.Maui.Services.Identity
{
	/// <summary>
	/// Provides identity services for Nostr Connect authentication in the Android app.
	/// </summary>
	public interface INativeIdentityService : IIdentityService
	{
		Task OnQrConnectReceived(string pubkey, List<string> relays, string secret, List<string> permissions);
	}
}