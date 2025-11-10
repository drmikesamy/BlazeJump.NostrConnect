using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Message;
using Newtonsoft.Json;
using BlazeJump.Tools.Models.NostrConnect;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Implements identity services for Nostr Connect authentication.
	/// </summary>
	public class IdentityService : IIdentityService
	{
		/// <summary>
		/// Occurs when a QR connect request is received.
		/// </summary>
		public event EventHandler<QrConnectEventArgs>? QrConnectReceived;

		/// <summary>
		/// Gets or sets the current platform.
		/// </summary>
		public PlatformEnum Platform { get; set; } = PlatformEnum.Web;

		private IRelayManager _relayManager {  get; set; }
		private IMessageService _messageService {  get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityService"/> class.
		/// </summary>
		/// <param name="relayManager">The relay manager for connection handling.</param>
		/// <param name="messageService">The message service for event handling.</param>
		public IdentityService(IRelayManager relayManager, IMessageService messageService) {
			_relayManager = relayManager;
			_messageService = messageService;
#if ANDROID
			Platform = PlatformEnum.Android;
#else
			Platform = PlatformEnum.Web;
#endif
		}

		/// <summary>
		/// Raises the QR connect received event.
		/// </summary>
		/// <param name="e">The event arguments containing connection details.</param>
		public void OnQrConnectReceived(QrConnectEventArgs e)
		{
			_ = SendNostrConnectReply(e.Pubkey, e.Secret);
		}
		private async Task SendNostrConnectReply(string theirPubKey, string secret)
		{
			var subscriptionHash = Guid.NewGuid().ToString();

			var message = new NostrConnectResponse
			{
				Id = subscriptionHash,
				Result = secret
			};

			var nEvent = _messageService.CreateNEvent(KindEnum.NostrConnect, JsonConvert.SerializeObject(message), null, null, new List<string> { theirPubKey });

			await _messageService.Send(KindEnum.NostrConnect, nEvent, theirPubKey);
		}

		/// <summary>
		/// Fetches the login scan response from relays.
		/// </summary>
		/// <param name="payload">The QR connect payload.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task FetchLoginScanResponse(QrConnectEventArgs payload)
		{
			var filter = new Filter
			{
				Kinds = new List<int> { (int)KindEnum.NostrConnect },
				Since = DateTime.Now.AddSeconds(-15),
				Until = DateTime.Now.AddSeconds(15),
				TaggedPublicKeys = new List<string> { payload.Pubkey }
			};

			var subscriptionHash = Guid.NewGuid().ToString();
			await _relayManager.QueryRelays(subscriptionHash, MessageTypeEnum.Req, new List<Filter> { filter }, 30000);
		}
	}

	/// <summary>
	/// Event arguments for QR connect authentication.
	/// </summary>
	public class QrConnectEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the relay URL.
		/// </summary>
		public string Relay { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the public key for authentication.
		/// </summary>
		public string Pubkey { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the secret for authentication.
		/// </summary>
		public string Secret { get; set; } = string.Empty;
	}
}
