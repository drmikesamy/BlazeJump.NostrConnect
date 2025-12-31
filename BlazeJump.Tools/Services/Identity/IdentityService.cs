using System.Diagnostics;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlazeJump.Tools.Models.NostrConnect;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Base class providing common functionality for Nostr Connect identity services.
	/// Handles bidirectional communication between web clients and native devices.
	/// </summary>
	public abstract class IdentityService : IIdentityService
	{
		#region Events

		/// <summary>
		/// Occurs when the state of a NostrConnect session changes.
		/// </summary>
		public event EventHandler<NostrConnectSession>? NotifySessionStateChanged;

		/// <summary>
		/// Occurs when a ping response (pong) is received from a remote peer.
		/// </summary>
		public event EventHandler<NostrConnectResponse>? PingReceived;

		#endregion

		#region Fields

		/// <summary>
		/// Cryptographic service for encryption and decryption operations.
		/// </summary>
		protected readonly ICryptoService _cryptoService;

		/// <summary>
		/// Message service for handling Nostr messages.
		/// </summary>
		protected readonly IMessageService _messageService;

		/// <summary>
		/// Relay manager for managing connections to Nostr relays.
		/// </summary>
		protected readonly IRelayManager _relayManager;

		/// <summary>
		/// Concurrent dictionary tracking pending NostrConnect requests by request ID.
		/// </summary>
		protected readonly ConcurrentDictionary<string, NostrConnectRequestContext> _pendingRequests = new ConcurrentDictionary<string, NostrConnectRequestContext>();

		/// <summary>
		/// Concurrent dictionary tracking active relay subscriptions by public key.
		/// </summary>
		private readonly ConcurrentDictionary<string, string> _activeSubscriptions = new();

		/// <summary>
		/// Gets the dictionary of user profiles indexed by public key.
		/// </summary>
		public Dictionary<string, UserProfile> UserProfiles { get; private set; } = new Dictionary<string, UserProfile>();

		/// <summary>
		/// Gets or sets the currently active user profile.
		/// </summary>
		public UserProfile? ActiveUserProfile { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityService"/> class.
		/// </summary>
		protected IdentityService(ICryptoService cryptoService, IMessageService messageService, IRelayManager relayManager)
		{
			_cryptoService = cryptoService;
			_messageService = messageService;
			_relayManager = relayManager;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Raises the NotifySessionStateChanged event when a session's state changes.
		/// </summary>
		/// <param name="session">The session that changed state.</param>
		protected void OnNotifySessionStateChanged(NostrConnectSession session)
		{
			NotifySessionStateChanged?.Invoke(this, session);
		}

		/// <summary>
		/// Handles incoming NostrConnect messages, decrypts them, and routes to appropriate request or response handlers.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="message">The incoming Nostr message.</param>
		protected async Task OnIncomingNostrConnectMessage(object? sender, NMessage message)
		{
			Console.WriteLine(JsonConvert.SerializeObject(message));
			if (message.Event == null) return; var content = message.Event.Content;
			var senderPubKey = message.Event.Pubkey;
			var recipientPubKey = message.Event.Tags?.SingleOrDefault()?.Value;

			if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(senderPubKey) || string.IsNullOrEmpty(recipientPubKey))
			{
				return;
			}

			var (request, response) = await DecryptNostrConnectMessage(content, senderPubKey, recipientPubKey);

			if (request != null)
			{
				Console.WriteLine(JsonConvert.SerializeObject(request));
				await HandleIncomingRequest(senderPubKey, request);
			}
			else if (response != null)
			{
				Console.WriteLine(JsonConvert.SerializeObject(response));
				await HandleIncomingResponse(senderPubKey, response);
			}
		}

		#endregion

		#region Incoming Request Handling

		/// <summary>
		/// Routes an incoming NostrConnect request to the appropriate handler based on the command type.
		/// </summary>
		/// <param name="theirPubkey">The pubkey of the sender</param>
		/// <param name="request">The NostrConnect request to route.</param>
		protected async Task HandleIncomingRequest(string theirPubkey, NostrConnectRequest request)
		{
			var session = GetSessionByTheirPubkey(theirPubkey);

			if (session == null)
				return;

			switch (request.Method)
			{
				case CommandEnum.Connect:
					await HandleConnectRequest(session, request);
					break;

				case CommandEnum.Disconnect:
					await HandleDisconnectRequest(session, request);
					break;

				case CommandEnum.Ping:
					await HandlePingRequest(session, request);
					break;

				case CommandEnum.SignEvent:
					await HandleSignEventRequest(session, request);
					break;

				case CommandEnum.GetPublicKey:
					await HandleGetPublicKeyRequest(session, request);
					break;

				case CommandEnum.Nip04Encrypt:
					await HandleNip04EncryptRequest(session, request);
					break;

				case CommandEnum.Nip04Decrypt:
					await HandleNip04DecryptRequest(session, request);
					break;

				case CommandEnum.Nip44Encrypt:
					await HandleNip44EncryptRequest(session, request);
					break;

				case CommandEnum.Nip44Decrypt:
					await HandleNip44DecryptRequest(session, request);
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Handles a connect request by setting the session as connected and sending an acknowledgment.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The connect request.</param>
		protected virtual async Task HandleConnectRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			session.SetConnected();
			await SendEncryptedResponse(session, request.Id, "ack");
		}

		/// <summary>
		/// Handles a ping request by sending a pong response.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The ping request.</param>
		protected virtual async Task HandlePingRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			await SendEncryptedResponse(session, request.Id, "pong");
		}

		/// <summary>
		/// Handles a sign event request by signing the provided event data.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The sign event request.</param>
		/// <returns>A JSON string representing the signed event.</returns>
		protected async Task<string> HandleSignEventRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			if (request.Params.Length == 0)
				throw new ArgumentException("SignEvent requires event data");

			var eventData = JsonConvert.DeserializeObject<NEvent>(request.Params[0]);

			if (eventData == null)
				throw new ArgumentException("Invalid event data");

			eventData.Pubkey = session.OurPubkey;

			var eventJson = JsonConvert.SerializeObject(new
			{
				id = eventData.Id,
				pubkey = eventData.Pubkey,
				created_at = eventData.Created_At,
				kind = (int?)eventData.Kind,
				tags = eventData.Tags,
				content = eventData.Content
			});

			var signature = await _cryptoService.Sign(eventJson, session.OurPubkey);
			eventData.Sig = signature;

			return JsonConvert.SerializeObject(eventData);
		}

		/// <summary>
		/// Handles a get public key request by returning the session's public key.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The get public key request.</param>
		/// <returns>The public key as a string.</returns>
		protected async Task<string> HandleGetPublicKeyRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			return session.OurPubkey;
		}

		/// <summary>
		/// Handles a NIP-04 encrypt request by encrypting the provided plaintext.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The NIP-04 encrypt request.</param>
		/// <returns>The encrypted ciphertext as a string.</returns>
		protected async Task<string> HandleNip04EncryptRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			if (request.Params.Length < 2)
				throw new ArgumentException("Nip04Encrypt requires plaintext and recipient pubkey");

			string plainText = request.Params[0];
			string recipientPubkey = request.Params[1];

			return await _cryptoService.Nip04Encrypt(plainText, recipientPubkey, session.OurPubkey);
		}

		/// <summary>
		/// Handles a NIP-04 decrypt request by decrypting the provided ciphertext.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The NIP-04 decrypt request.</param>
		/// <returns>The decrypted plaintext as a string.</returns>
		protected async Task<string> HandleNip04DecryptRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			if (request.Params.Length < 2)
				throw new ArgumentException("Nip04Decrypt requires ciphertext and sender pubkey");

			string cipherText = request.Params[0];

			return await _cryptoService.Nip04Decrypt(cipherText, session.TheirPubkey, session.OurPubkey);
		}

		/// <summary>
		/// Handles a NIP-44 encrypt request by encrypting the provided plaintext.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The NIP-44 encrypt request.</param>
		/// <returns>The encrypted ciphertext as a string.</returns>
		protected async Task<string> HandleNip44EncryptRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			if (request.Params.Length < 2)
				throw new ArgumentException("Nip44Encrypt requires plaintext and recipient pubkey");

			string plainText = request.Params[0];

			return await _cryptoService.Nip44Encrypt(plainText, session.TheirPubkey, session.OurPubkey);
		}

		/// <summary>
		/// Handles a NIP-44 decrypt request by decrypting the provided ciphertext.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The NIP-44 decrypt request.</param>
		/// <returns>The decrypted plaintext as a string.</returns>
		protected async Task<string> HandleNip44DecryptRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			if (request.Params.Length < 2)
				throw new ArgumentException("Nip44Decrypt requires ciphertext and sender pubkey");

			string cipherText = request.Params[0];

			return await _cryptoService.Nip44Decrypt(cipherText, session.TheirPubkey, session.OurPubkey);
		}

		/// <summary>
		/// Handles a disconnect request by removing the session.
		/// </summary>
		/// <param name="session">The session that received the request.</param>
		/// <param name="request">The disconnect request.</param>
		/// <returns>An acknowledgment string.</returns>
		protected virtual async Task HandleDisconnectRequest(NostrConnectSession session, NostrConnectRequest request)
		{
			await SendEncryptedResponse(session, request.Id, "ack");
			ActiveUserProfile!.Sessions.Remove(session);
			OnNotifySessionStateChanged(session);
		}

		#endregion
		#region Incoming Response Handling

		/// <summary>
		/// Routes an incoming NostrConnect response to the appropriate handler based on the command type.
		/// </summary>
		/// <param name="theirPubkey">The pubkey of the sender</param>
		/// <param name="response">The NostrConnect response to route.</param>
		protected async Task HandleIncomingResponse(string theirPubkey, NostrConnectResponse response)
		{
			if (!_pendingRequests.TryRemove(response.Id, out var requestContext))
			{
				return;
			}

			var session = ActiveUserProfile!.Sessions.SingleOrDefault(s => s.SessionId == requestContext.SessionId);

			if (session == null)
			{
				return;
			}

			switch (requestContext.Command)
			{
				case CommandEnum.Connect:
					Console.WriteLine($"Connect response with {session.Secret} received");
					session.SetTheirPubkey(theirPubkey);
					await HandleConnectResponse(session, response);
					break;

				case CommandEnum.Ping:
					await HandlePingResponse(session, response);
					break;

				case CommandEnum.Disconnect:
					await HandleDisconnectResponse(session, response);
					break;

				case CommandEnum.SignEvent:
					await HandleSignEventResponse(session, response);
					break;

				case CommandEnum.GetPublicKey:
					await HandleGetPublicKeyResponse(session, response);
					break;

				case CommandEnum.Nip04Encrypt:
					await HandleNip04EncryptResponse(session, response);
					break;

				case CommandEnum.Nip04Decrypt:
					await HandleNip04DecryptResponse(session, response);
					break;

				case CommandEnum.Nip44Encrypt:
					await HandleNip44EncryptResponse(session, response);
					break;

				case CommandEnum.Nip44Decrypt:
					await HandleNip44DecryptResponse(session, response);
					break;

				default:
					break;
			}
			OnNotifySessionStateChanged(session);
		}

		/// <summary>
		/// Handles a connect response by setting the session as connected if the result is "ack".
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The connect response.</param>
		protected virtual async Task HandleConnectResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			session.SetConnected();
			await SendPing(session);
		}

		/// <summary>
		/// Handles a ping response by setting the session as connected.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The ping response.</param>
		protected virtual Task HandlePingResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			session.SetConnected();
			PingReceived?.Invoke(this, response);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a disconnect response by removing the session and cleaning up.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The disconnect response.</param>
		protected virtual async Task HandleDisconnectResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			if (response.Result == "ack")
			{
				ActiveUserProfile!.Sessions.Remove(session);
			}
		}

		/// <summary>
		/// Handles a sign event response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The sign event response.</param>
		protected virtual Task HandleSignEventResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a get public key response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The get public key response.</param>
		protected virtual Task HandleGetPublicKeyResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a NIP-04 encrypt response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The NIP-04 encrypt response.</param>
		protected virtual Task HandleNip04EncryptResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a NIP-04 decrypt response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The NIP-04 decrypt response.</param>
		protected virtual Task HandleNip04DecryptResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a NIP-44 encrypt response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The NIP-44 encrypt response.</param>
		protected virtual Task HandleNip44EncryptResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles a NIP-44 decrypt response. Default implementation does nothing.
		/// </summary>
		/// <param name="session">The session that received the response.</param>
		/// <param name="response">The NIP-44 decrypt response.</param>
		protected virtual Task HandleNip44DecryptResponse(NostrConnectSession session, NostrConnectResponse response)
		{
			return Task.CompletedTask;
		}

		#endregion

		#region Set Up Listener

		/// <summary>
		/// Listens for Nostr Connect messages on specified relays for a given public key
		/// </summary>
		public async Task ListenForNostrConnectMessages(List<string> relays, string pubkey)
		{
			if (_activeSubscriptions.ContainsKey(pubkey))
			{
				return;
			}

			foreach (var relay in relays)
			{
				_relayManager.TryAddUri(relay);
			}

			var filter = new Filter
			{
				Kinds = new List<int> { (int)KindEnum.NostrConnect },
				Since = DateTime.UtcNow.AddSeconds(-30),
				TaggedPublicKeys = new List<string> { pubkey }
			};

			var subscriptionId = $"nostr-connect-{pubkey.Substring(0, 16)}";

			await _relayManager.QueryRelays(subscriptionId, MessageTypeEnum.Req, new List<Filter> { filter }, 60000);
			_activeSubscriptions.TryAdd(pubkey, subscriptionId);
		}

		#endregion

		#region Decrypt Incoming

		/// <summary>
		/// Decrypts and deserializes an encrypted NostrConnect message.
		/// Returns both the request and response objects (one will be null).
		/// </summary>
		protected async Task<(NostrConnectRequest? request, NostrConnectResponse? response)> DecryptNostrConnectMessage(
			string encryptedContent,
			string senderPubkey,
			string recipientPubkey)
		{
			try
			{
				var decrypted = await _cryptoService.Nip44Decrypt(encryptedContent, senderPubkey, recipientPubkey);

				var jObject = JObject.Parse(decrypted);

				if (jObject.ContainsKey("method"))
				{
					var request = JsonConvert.DeserializeObject<NostrConnectRequest>(decrypted);
					if (request != null && !string.IsNullOrEmpty(request.Id))
					{
						return (request, null);
					}
				}
				else
				{
					var response = JsonConvert.DeserializeObject<NostrConnectResponse>(decrypted);
					if (response != null && !string.IsNullOrEmpty(response.Id))
					{
						return (null, response);
					}
				}
			}
			catch (Exception)
			{
			}

			return (null, null);
		}

		#endregion

		#region Sending Messages

		/// <summary>
		/// Sends an encrypted NostrConnect request to the specified recipient.
		/// Used for initiating commands from either client or device.
		/// </summary>
		protected async Task SendEncryptedRequest(
			NostrConnectSession session,
			CommandEnum command,
			List<string> parameters)
		{
			var requestId = Guid.NewGuid().ToString();

			var context = new NostrConnectRequestContext
			{
				SessionId = session.SessionId,
				Command = command,
				TargetPubkey = session.TheirPubkey,
				Parameters = parameters.ToArray(),
				CreatedAt = DateTime.UtcNow
			};
			_pendingRequests.TryAdd(requestId, context);

			var request = new NostrConnectRequest
			{
				Id = requestId,
				Method = command,
				Params = parameters.ToArray()
			};

			var requestJson = JsonConvert.SerializeObject(request);
			var encrypted = await _cryptoService.Nip44Encrypt(requestJson, session.TheirPubkey, session.OurPubkey);

			var nEvent = _messageService.CreateNEvent(
				session.OurPubkey,
				KindEnum.NostrConnect,
				encrypted,
				null,
				null,
				new List<string> { session.TheirPubkey }
			);

			await _messageService.Send(KindEnum.NostrConnect, nEvent, null);
		}

		/// <summary>
		/// Sends an encrypted NostrConnect response back to the requester.
		/// Used for replying to commands received from either client or device.
		/// </summary>
		protected async Task SendEncryptedResponse(
			NostrConnectSession session,
			string requestId,
			string result,
			string error = "")
		{
			var response = new NostrConnectResponse
			{
				Id = requestId,
				Result = result,
				Error = error
			};

			var responseJson = JsonConvert.SerializeObject(response);
			var encrypted = await _cryptoService.Nip44Encrypt(responseJson, session.TheirPubkey, session.OurPubkey);

			var nEvent = _messageService.CreateNEvent(
				session.OurPubkey,
				KindEnum.NostrConnect,
				encrypted,
				null,
				null,
				new List<string> { session.TheirPubkey }
			);

			await _messageService.Send(KindEnum.NostrConnect, nEvent, null);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sends a ping request to the specified session.
		/// </summary>
		/// <param name="session">The session to ping.</param>
		public async Task SendPing(NostrConnectSession session)
		{
			await SendEncryptedRequest(session, CommandEnum.Ping, new List<string>());
		}

		/// <summary>
		/// Sends a disconnect request to the specified session.
		/// </summary>
		/// <param name="session">The session to disconnect.</param>
		public async Task SendDisconnect(NostrConnectSession session)
		{
			await SendEncryptedRequest(session, CommandEnum.Disconnect, new List<string>());
		}

		/// <summary>
		/// Creates a new user profile with an optional private key.
		/// </summary>
		/// <param name="privateKey">The optional private key to use; if null, a new key pair is generated.</param>
		public async Task CreateUserProfile(string? privateKey = null)
		{
			string newPubKey;
			if (!string.IsNullOrEmpty(privateKey))
			{
				newPubKey = await _cryptoService.GenerateKeyPair(privateKey);
			}
			else
			{
				newPubKey = await _cryptoService.GenerateKeyPair();
			}

			var userProfile = new UserProfile
			{
				PublicKey = newPubKey,
				IsCurrentUser = true,
				LastUpdated = DateTime.UtcNow
			};

			UserProfiles.TryAdd(newPubKey, userProfile);
			ActiveUserProfile = userProfile;
		}

		#endregion

		#region Session Management

		/// <summary>
		/// Removes a session from the active user profile.
		/// </summary>
		/// <param name="session">The session to remove.</param>
		private void RemoveSession(NostrConnectSession session)
		{
			ActiveUserProfile!.Sessions.Remove(session);
		}

		/// <summary>
		/// Retrieves a session from the active user profile by the remote party's public key.
		/// </summary>
		/// <param name="theirPubkey">The public key of the remote party.</param>
		/// <returns>The matching NostrConnect session, or null if not found.</returns>
		protected NostrConnectSession? GetSessionByTheirPubkey(string theirPubkey)
		{
			return ActiveUserProfile!.Sessions.SingleOrDefault(s => s.TheirPubkey == theirPubkey);
		}

		#endregion
	}
}
