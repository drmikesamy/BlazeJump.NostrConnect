using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Message;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Services.Identity
{
	/// <summary>
	/// Implements Nostr Connect (NIP-46) protocol services for remote signing.
	/// </summary>
	public class NostrConnectService : INostrConnectService
	{
		/// <summary>
		/// Occurs when a signing request is received and needs approval.
		/// </summary>
		public event EventHandler<NostrConnectRequestEventArgs>? SigningRequestReceived;

		private readonly ICryptoService _cryptoService;
		private readonly IMessageService _messageService;
		private readonly IRelayManager _relayManager;
		private readonly Dictionary<string, PendingRequest> _pendingRequests = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="NostrConnectService"/> class.
		/// </summary>
		/// <param name="cryptoService">The crypto service for signing operations.</param>
		/// <param name="messageService">The message service for event creation.</param>
		/// <param name="relayManager">The relay manager for communication.</param>
		public NostrConnectService(
			ICryptoService cryptoService,
			IMessageService messageService,
			IRelayManager relayManager)
		{
			_cryptoService = cryptoService;
			_messageService = messageService;
			_relayManager = relayManager;
		}

		/// <summary>
		/// Generates a connection URI for QR code display (bunker:// protocol).
		/// </summary>
		/// <param name="relay">The relay URL to use for communication.</param>
		/// <returns>The bunker:// URI string.</returns>
		public async Task<string> GenerateConnectionUri(string relay)
		{
			// Ensure permanent key pair exists
			if (_cryptoService.PermanentPublicKeyHex == null)
			{
				await _cryptoService.CreateOrLoadPermanentKeyPair();
			}

			var pubkeyHex = _cryptoService.PermanentPublicKeyHex;
			
			// Format: bunker://<pubkey>?relay=<relay_url>
			return $"bunker://{pubkeyHex}?relay={Uri.EscapeDataString(relay)}";
		}

		/// <summary>
		/// Handles an incoming Nostr Connect request.
		/// </summary>
		/// <param name="request">The decrypted request.</param>
		/// <param name="requesterPubkey">The public key of the requester.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task HandleRequest(NostrConnectRequest request, string requesterPubkey)
		{
			// Store the pending request
			_pendingRequests[request.Id] = new PendingRequest
			{
				Request = request,
				RequesterPubkey = requesterPubkey,
				Timestamp = DateTime.UtcNow
			};

			// Handle different methods
			switch (request.Method.ToLower())
			{
				case "connect":
					// Auto-approve connect requests
					await RespondToSigningRequest(request.Id, true);
					break;

				case "sign_event":
				case "get_public_key":
				case "nip04_encrypt":
				case "nip04_decrypt":
				case "nip44_encrypt":
				case "nip44_decrypt":
					// Raise event for user approval
					SigningRequestReceived?.Invoke(this, new NostrConnectRequestEventArgs
					{
						RequestId = request.Id,
						Method = request.Method,
						Params = request.Params,
						RequesterPubkey = requesterPubkey
					});
					break;

				default:
					// Unknown method - send error response
					await SendErrorResponse(request.Id, requesterPubkey, $"Unknown method: {request.Method}");
					break;
			}
		}

		/// <summary>
		/// Approves and executes a signing request.
		/// </summary>
		/// <param name="requestId">The request ID to approve.</param>
		/// <param name="approved">Whether the request is approved.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task RespondToSigningRequest(string requestId, bool approved)
		{
			if (!_pendingRequests.TryGetValue(requestId, out var pendingRequest))
			{
				throw new InvalidOperationException($"No pending request found with ID: {requestId}");
			}

			try
			{
				if (!approved)
				{
					await SendErrorResponse(requestId, pendingRequest.RequesterPubkey, "User rejected the request");
					return;
				}

				var request = pendingRequest.Request;
				string result = string.Empty;

				// Execute the requested method
				switch (request.Method.ToLower())
				{
					case "connect":
						result = "ack";
						break;

					case "get_public_key":
						result = _cryptoService.PermanentPublicKeyHex ?? string.Empty;
						break;

					case "sign_event":
						result = await HandleSignEvent(request.Params);
						break;

					case "nip04_encrypt":
						result = await HandleNip04Encrypt(request.Params);
						break;

					case "nip04_decrypt":
						result = await HandleNip04Decrypt(request.Params);
						break;

					default:
						await SendErrorResponse(requestId, pendingRequest.RequesterPubkey, $"Method not implemented: {request.Method}");
						return;
				}

				// Send success response
				await SendSuccessResponse(requestId, pendingRequest.RequesterPubkey, result);
			}
			finally
			{
				// Clean up pending request
				_pendingRequests.Remove(requestId);
			}
		}

		private async Task<string> HandleSignEvent(List<string> parameters)
		{
			if (parameters.Count == 0)
				throw new ArgumentException("sign_event requires an event parameter");

			var eventJson = parameters[0];
			var eventToSign = JsonConvert.DeserializeObject<NEvent>(eventJson);
			
			if (eventToSign == null)
				throw new ArgumentException("Invalid event JSON");

			// Sign the event with permanent key
			var signature = await _cryptoService.Sign(eventToSign.Id, ethereal: false);
			eventToSign.Sig = signature;

			return JsonConvert.SerializeObject(eventToSign);
		}

		private async Task<string> HandleNip04Encrypt(List<string> parameters)
		{
			if (parameters.Count < 2)
				throw new ArgumentException("nip04_encrypt requires pubkey and plaintext parameters");

			var theirPubkey = parameters[0];
			var plaintext = parameters[1];

			var encrypted = await _cryptoService.AesEncrypt(plaintext, theirPubkey, ethereal: false);
			return $"{encrypted.CipherText}?iv={encrypted.Iv}";
		}

		private async Task<string> HandleNip04Decrypt(List<string> parameters)
		{
			if (parameters.Count < 2)
				throw new ArgumentException("nip04_decrypt requires pubkey and ciphertext parameters");

			var theirPubkey = parameters[0];
			var cipherWithIv = parameters[1];
			
			var parts = cipherWithIv.Split("?iv=");
			if (parts.Length != 2)
				throw new ArgumentException("Invalid ciphertext format");

			return await _cryptoService.AesDecrypt(parts[0], theirPubkey, parts[1], ethereal: false);
		}

		private async Task SendSuccessResponse(string requestId, string requesterPubkey, string result)
		{
			var response = new NostrConnectResponse
			{
				Id = requestId,
				Result = result
			};

			await SendResponse(response, requesterPubkey);
		}

		private async Task SendErrorResponse(string requestId, string requesterPubkey, string error)
		{
			var response = new NostrConnectResponse
			{
				Id = requestId,
				Error = error
			};

			await SendResponse(response, requesterPubkey);
		}

		private async Task SendResponse(NostrConnectResponse response, string requesterPubkey)
		{
			var responseJson = JsonConvert.SerializeObject(response);
			var nEvent = _messageService.CreateNEvent(
				KindEnum.NostrConnect,
				responseJson,
				null,
				null,
				new List<string> { requesterPubkey }
			);

			await _messageService.Send(KindEnum.NostrConnect, nEvent, requesterPubkey);
		}

		private class PendingRequest
		{
			public NostrConnectRequest Request { get; set; } = new();
			public string RequesterPubkey { get; set; } = string.Empty;
			public DateTime Timestamp { get; set; }
		}
	}
}
