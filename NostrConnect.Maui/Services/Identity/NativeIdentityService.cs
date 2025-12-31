using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Logging;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Services.Persistence;
using Newtonsoft.Json;

namespace NostrConnect.Maui.Services.Identity
{
	public class NativeIdentityService : IdentityService, INativeIdentityService
	{
		private readonly ILoggingService _loggingService;
		private readonly INostrDataService _dataService;

		public NativeIdentityService(
			IRelayManager relayManager,
			IMessageService messageService,
			ICryptoService cryptoService,
			ILoggingService loggingService,
			INostrDataService dataService)
			: base(cryptoService, messageService, relayManager)
		{
			_loggingService = loggingService;
			_dataService = dataService;
			_messageService.ProcessNostrConnectMessage += async (sender, message) => await OnIncomingNostrConnectMessage(sender, message);
		}

		public async Task OnQrConnectReceived(string theirPubkey, List<string> relays, string secret, List<string> permissions)
		{
			if (ActiveUserProfile == null)
				return;

			await ListenForNostrConnectMessages(relays, ActiveUserProfile.PublicKey);

			var existingSession = ActiveUserProfile.Sessions.FirstOrDefault(s => s.TheirPubkey == theirPubkey);

			NostrConnectSession session;

			if (existingSession != null)
			{
				session = existingSession;
				session.Secret = secret;
				session.Permissions = permissions ?? new List<string>();
			}
			else
			{
				session = new NostrConnectSession(ActiveUserProfile.PublicKey)
				{
					Secret = secret,
					Relays = relays,
					Permissions = permissions ?? new List<string>()
				};
				session.SetTheirPubkey(theirPubkey);
				session.StatusChanged += (s, status) => OnNotifySessionStateChanged(session);
				ActiveUserProfile.Sessions.Add(session);
			}

			await SendEncryptedResponse(session, secret, secret);
			session.SetConnected();
			_loggingService.Log($"Session connected with {theirPubkey.Substring(0, 8)}");
			OnNotifySessionStateChanged(session);
		}

		protected override Task HandlePingResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			_loggingService.Log($"Pong received from {session.TheirPubkey.Substring(0, 8)}");
			return base.HandlePingResponse(session, response);
		}
	}
}