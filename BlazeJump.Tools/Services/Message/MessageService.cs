using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using BlazeJump.Tools.Builders;
using BlazeJump.Tools.Helpers;
using BlazeJump.Tools.Services.Logging;
using BlazeJump.Tools.Services.Notification;
using BlazeJump.Helpers;
using AutoMapper;
using System.Collections.Concurrent;
using BlazeJump.Tools.Services.Identity;

namespace BlazeJump.Tools.Services.Message
{
	/// <summary>
	/// Implements message services for fetching, verifying, and sending Nostr events.
	/// </summary>
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private INotificationService _notificationService;
		private IMapper _mapper;

		/// <summary>
		/// Triggers an event to process a new nostr connect login.
		/// </summary>
		public event EventHandler<NMessage>? ProcessNostrConnectMessage;

		/// <summary>
		/// Gets or sets the message store for caching events.
		/// </summary>
		public ConcurrentDictionary<string, NMessage> MessageStore { get; set; } = new();

		/// <summary>
		/// Gets or sets the relation register for tracking user relationships.
		/// </summary>
		public RelationRegister RelationRegister { get; set; } = new();

		/// <summary>
		/// Gets or sets the queue of events awaiting metadata.
		/// </summary>
		public Queue<string> EventsAwaitingMetaData { get; set; } = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageService"/> class.
		/// </summary>
		/// <param name="relayManager">The relay manager for connection handling.</param>
		/// <param name="cryptoService">The crypto service for encryption and signing.</param>
		/// <param name="notificationService">The notification service.</param>
		/// <param name="mapper">The AutoMapper instance.</param>
		/// <param name="loggingService">The logging service.</param>
		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, INotificationService notificationService, IMapper mapper, ILoggingService loggingService)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_notificationService = notificationService;
			_mapper = mapper;
			_relayManager.ProcessMessageQueue += ProcessReceivedMessages;
		}

		/// <summary>
		/// Looks up a user by search string.
		/// </summary>
		/// <param name="searchString">The search string (npub, hex, or name).</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task LookupUser(string searchString)
		{
			if (RelationRegister.TryGetRelation(searchString, RelationTypeEnum.SearchToSubscriptionId, out var _))
			{
				return;
			}

			var filterBuilder = new FilterBuilder();
			var filterList = filterBuilder
				.AddFilter()
				.AddKind(KindEnum.Metadata)
				.AddSearch(searchString)
				.WithLimit(10)
				.Build();
			var lookupGuid = Guid.NewGuid().ToString();
			RelationRegister.AddRelation(lookupGuid, RelationTypeEnum.SubscriptionGuidToIds, lookupGuid);
			RelationRegister.AddRelation(searchString, RelationTypeEnum.SearchToSubscriptionId, lookupGuid);
			await Fetch(filterList, lookupGuid);
		}

		/// <summary>
		/// Fetches a page of events for a user.
		/// </summary>
		/// <param name="hex">The user's hex public key.</param>
		/// <param name="untilMarker">Optional timestamp to fetch events until.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task FetchPage(string hex, DateTime? untilMarker = null)
		{
			var until = untilMarker ?? DateTime.Now.AddDays(1);

			FilterBuilder filters = new();
			filters
				.AddFilter()
				.AddKind(KindEnum.Text)
				.AddEventId(hex);
			filters
				.AddFilter()
				.AddKind(KindEnum.Text)
				.WithToDate(until)
				.AddTaggedEventId(hex);
			filters
				.AddFilter()
				.AddKind(KindEnum.Text)
				.WithToDate(until)
				.WithLimit(5)
				.AddAuthor(hex);
			var filterList = filters.Build();
			var subscriptionId = Guid.NewGuid().ToString();
			RelationRegister.AddRelation(subscriptionId, RelationTypeEnum.RootLevelSubscription, hex);
			await Fetch(filterList, subscriptionId);
		}

		/// <summary>
		/// Fetches events matching the specified filters.
		/// </summary>
		/// <param name="filters">The list of filters to apply.</param>
		/// <param name="subscriptionId">Optional subscription identifier.</param>
		/// <param name="messageType">Optional message type.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task Fetch(List<Filter> filters, string? subscriptionId = null,
			MessageTypeEnum? messageType = null)
		{
			if (filters.Any())
			{
				await _relayManager.QueryRelays(subscriptionId ?? Guid.NewGuid().ToString(), messageType ?? MessageTypeEnum.Req, filters);
			}
		}

		private void ProcessReceivedMessages(object? sender, EventArgs e)
		{
			while (_relayManager.ReceivedMessages.TryDequeue(out var message))
			{
				if (message.MessageType == MessageTypeEnum.Eose)
				{
					_relayManager.ReceivedMessages.TryDequeue(out _);
					if (message.SubscriptionId != null)
					{
						EndOfFetch(message.SubscriptionId);
					}
					_notificationService.UpdateTheState();
					continue;
				}

				if (message.Event == null)
				{
					break;
				}

				_relayManager.ReceivedMessages.TryDequeue(out _);

				switch (message.Event.Kind)
				{
					case KindEnum.Metadata:
						if (message.Event.Pubkey != null)
						{
							MessageStore.TryAdd(message.Event.Pubkey, message);
						}
						break;
					case KindEnum.NostrConnect:
						ProcessNostrConnectMessage?.Invoke(this, message);
						break;
					default:
						if (message.Event.Id != null)
						{
							MessageStore.TryAdd(message.Event.Id, message);
						}
						break;
				}

				lock (RelationRegister)
				{
					ProcessRelations(message);
				}
			}
		}


		private void ProcessRelations(NMessage message)
		{
			lock (RelationRegister)
			{
				if (message.Event?.Pubkey != null && message.Event.Id != null)
				{
					RelationRegister.AddRelation(message.Event.Pubkey, RelationTypeEnum.EventsByUser, message.Event.Id);
					RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.UserByEvent, message.Event.Pubkey);
				}

				if (message.SubscriptionId != null &&
					RelationRegister.TryGetRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds, out var _) &&
					message.Event?.Pubkey != null)
				{
					RelationRegister.AddRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds, message.Event.Pubkey);
				}

				if (message.Event?.Content != null)
				{
					foreach (var item in ParseEmbeds.ParseEmbeddedStrings(message.Event.Content))
					{
						if (item.Key.Contains("nevent"))
						{
							foreach (var bech32 in item.Value)
							{
								var nEventHex = GeneralHelpers.Bech32ToTLVComponents(bech32, Bech32PrefixEnum.nevent);
								var idFound = nEventHex.TryGetValue(TLVTypeEnum.Special, out string? id);
								var userIdFound = nEventHex.TryGetValue(TLVTypeEnum.Author, out string? userId);
								if (idFound && id != null && message.Event.Id != null)
								{
									RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.ReferencedEvent, id);
								}
								if (userIdFound && userId != null && message.Event.Id != null)
								{
									RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.UserByEvent, userId);
								}
							}
						}
					}
				}

				if (message.Event != null)
				{
					foreach (var tag in message.Event.Tags!)
					{
						if (tag.Value != null && message.Event.Id != null)
						{
							switch (tag.Key)
							{
								case TagEnum.e:
									RelationRegister.AddRelation(tag.Value, RelationTypeEnum.EventChildren, message.Event.Id);
									if (tag.Value3 == "Root")
									{
										RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.EventRoot, tag.Value);
									}
									if (tag.Value3 == "Reply")
									{
										RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.EventParent, tag.Value);
									}
									break;
								default:
									break;
							}
						}
					}
				}
			}
		}
		private void EndOfFetch(string subscriptionId)
		{
			FilterBuilder filterBuilder = new();
			lock (RelationRegister)
			{
				if (RelationRegister.TryGetRelation(subscriptionId, RelationTypeEnum.RootLevelSubscription, out var rootId)
					&& (RelationRegister.TryGetRelations(rootId, RelationTypeEnum.EventChildren, out var topLevelEventIds)
					|| RelationRegister.TryGetRelations(rootId, RelationTypeEnum.EventsByUser, out topLevelEventIds)))
				{
					filterBuilder
						.AddFilter()
						.AddKind(KindEnum.Text)
						.AddTaggedEventIds(topLevelEventIds.Distinct().ToList());

					if (RelationRegister.TryGetRelations(topLevelEventIds, RelationTypeEnum.ReferencedEvent, out var referencedEventIds))
					{
						filterBuilder
							.AddFilter()
							.AddKind(KindEnum.Text)
							.AddEventIds(referencedEventIds.Distinct().ToList());
						foreach (var relatedEventId in referencedEventIds)
						{
							EventsAwaitingMetaData.Enqueue(relatedEventId);
						}
					}

					topLevelEventIds.Add(rootId.First());

					if (RelationRegister.TryGetRelations(topLevelEventIds, RelationTypeEnum.UserByEvent, out var userIds))
					{
						filterBuilder
							.AddFilter()
							.AddKind(KindEnum.Metadata)
							.AddAuthors(userIds.Where(id => !MessageStore.ContainsKey(id)).Distinct().ToList());
					}
				}
			}

			if (EventsAwaitingMetaData.Any())
			{
				var pendingUserIds = new List<string>();
				while (EventsAwaitingMetaData.Count > 0)
				{
					var pendingEventId = EventsAwaitingMetaData.Dequeue();
					if (MessageStore.TryGetValue(pendingEventId, out var eventAwaitingMetadata) &&
						eventAwaitingMetadata.Event?.Pubkey != null)
					{
						pendingUserIds.Add(eventAwaitingMetadata.Event.Pubkey);
					}
				}
				filterBuilder
					.AddFilter()
					.AddKind(KindEnum.Metadata)
					.AddAuthors(pendingUserIds);
			}

			var filters = filterBuilder.Build();
			var fetchId = Guid.NewGuid().ToString();
			_ = Fetch(filters, fetchId);
		}

		private async Task<bool> Sign(NEvent nEvent)
		{
			if (string.IsNullOrEmpty(nEvent.Pubkey))
				return false;
			var signableEvent = _mapper.Map<NEvent, SignableNEvent>(nEvent);
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			nEvent.Id = Convert.ToHexString(serialisedNEvent.SHA256Hash()).ToLower();
			var signature = await _cryptoService.Sign(serialisedNEvent, nEvent.Pubkey);
			nEvent.Sig = signature;
			return true;
		}

		/// <summary>
		/// Verifies a Nostr event's signature.
		/// </summary>
		/// <param name="nEvent">The event to verify.</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		public bool Verify(NEvent nEvent)
		{
			if (nEvent.Sig == null || nEvent.Pubkey == null)
			{
				return false;
			}
			var signableEvent = _mapper.Map<NEvent, SignableNEvent>(nEvent);
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.Pubkey);
			return verified;
		}

		/// <summary>
		/// Sends a Nostr event to relays.
		/// </summary>
		/// <param name="kind">The event kind.</param>
		/// <param name="nEvent">The event to send.</param>
		/// <param name="encryptPubKey">Optional public key for encryption.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task Send(KindEnum kind, NEvent nEvent, string? encryptPubKey = null)
		{
			if (!String.IsNullOrEmpty(encryptPubKey) && !String.IsNullOrEmpty(nEvent.Content) && !String.IsNullOrEmpty(nEvent.Pubkey))
			{
				var encryptedContent = await _cryptoService.Nip44Encrypt(nEvent.Content, encryptPubKey, nEvent.Pubkey);
				nEvent.Content = JsonConvert.SerializeObject(encryptedContent);
			}
			await Sign(nEvent);
			var subscriptionHash = Guid.NewGuid().ToString();
			await _relayManager.SendNEvent(nEvent, subscriptionHash);
		}

		/// <summary>
		/// Creates a new Nostr event.
		/// </summary>
		/// <param name="pubkey">The event public key.</param>
		/// <param name="kind">The event kind.</param>
		/// <param name="message">The event message content.</param>
		/// <param name="parentId">Optional parent event ID.</param>
		/// <param name="rootId">Optional root event ID.</param>
		/// <param name="ptags">Optional list of public key tags.</param>
		/// <returns>The created Nostr event.</returns>
		public NEvent CreateNEvent(string pubkey, KindEnum kind, string message, string? parentId = null, string? rootId = null, List<string>? ptags = null)
		{
			var newEvent = new NEvent
			{
				Pubkey = pubkey,
				Kind = kind,
				Content = message,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
			if (parentId != null)
			{
				newEvent!.Tags!.Add(new EventTag
				{
					Key = TagEnum.e,
					Value = parentId,
					Value3 = "reply"
				});
			}
			if (rootId != null)
			{
				newEvent!.Tags!.Add(new EventTag
				{
					Key = TagEnum.e,
					Value = rootId,
					Value3 = "root"
				});
			}
			if (ptags != null)
			{
				foreach (var ptag in ptags)
				{
					newEvent!.Tags!.Add(new EventTag
					{
						Key = TagEnum.p,
						Value = ptag
					});
				}
			}
			return newEvent;
		}
	}
}