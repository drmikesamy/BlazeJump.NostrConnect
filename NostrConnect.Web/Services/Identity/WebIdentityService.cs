using BlazeJump.Tools.Builders;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Services.Logging;
using Newtonsoft.Json;
using BlazeJump.Tools.Services.Crypto;

namespace NostrConnect.Web.Services.Identity
{
	public class WebIdentityService : IdentityService, IWebIdentityService
	{
		public WebIdentityService(
			IRelayManager relayManager,
			IMessageService messageService,
			ICryptoService cryptoService,
			ILoggingService loggingService)
			: base(cryptoService, messageService, relayManager)
		{
			_messageService.ProcessNostrConnectMessage += async (sender, message) => await OnIncomingNostrConnectMessage(sender, message);
		}

		public async Task<string> CreateNewSession()
		{
			string webPubKey;

			var existingKey = await _cryptoService.GetExistingPublicKey();

			if (!string.IsNullOrEmpty(existingKey))
			{
				webPubKey = existingKey;
			}
			else
			{
				webPubKey = await _cryptoService.GenerateKeyPair();
			}

			var builder = new NostrConnectUriBuilder()
				.WithClientPubKey(webPubKey)
				.AddRelays(_relayManager.Relays)
				.WithRandomSecret();
			var connectionUrl = builder.Build();

			var session = new NostrConnectSession(webPubKey);
			session.Secret = builder.GetSecret();
			session.StatusChanged += (s, status) => OnNotifySessionStateChanged((NostrConnectSession)s!);
			ActiveUserProfile!.Sessions.Add(session);

			var context = new NostrConnectRequestContext
			{
				SessionId = session.SessionId,
				Command = CommandEnum.Connect,
				CreatedAt = DateTime.UtcNow
			};
			_pendingRequests.TryAdd(session.Secret, context);

			await ListenForNostrConnectMessages(builder.GetRelays(), webPubKey);
			return connectionUrl;
		}

		public async Task RequestSignEvent(NEvent eventToSign, NostrConnectSession session)
		{
			var parameters = new List<string> { JsonConvert.SerializeObject(eventToSign) };
			await SendEncryptedRequest(session, CommandEnum.SignEvent, parameters);
		}

		public async Task RequestPublicKey(NostrConnectSession session)
		{
			await SendEncryptedRequest(session, CommandEnum.GetPublicKey, new List<string>());
		}
	}
}