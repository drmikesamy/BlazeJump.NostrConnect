using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections.Events;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Message
{
	/// <summary>
	/// Provides message services for fetching, verifying, and sending Nostr events.
	/// </summary>
	public interface IMessageService
	{
		/// <summary>
		/// Looks up a user by search string.
		/// </summary>
		/// <param name="searchString">The search string (npub, hex, or name).</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task LookupUser(string searchString);

		/// <summary>
		/// Fetches a page of events for a user.
		/// </summary>
		/// <param name="hex">The user's hex public key.</param>
		/// <param name="untilMarker">Optional timestamp to fetch events until.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task FetchPage(string hex, DateTime? untilMarker = null);

		/// <summary>
		/// Gets or sets the relation register for tracking user relationships.
		/// </summary>
		RelationRegister RelationRegister { get; set; }

		/// <summary>
		/// Gets or sets the message store for caching events.
		/// </summary>
		ConcurrentDictionary<string, NMessage> MessageStore { get; set; }

		/// <summary>
		/// Fetches events matching the specified filters.
		/// </summary>
		/// <param name="filters">The list of filters to apply.</param>
		/// <param name="subscriptionId">Optional subscription identifier.</param>
		/// <param name="messageType">Optional message type.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Fetch(List<Filter> filters, string? subscriptionId = null, MessageTypeEnum? messageType = null);

		/// <summary>
		/// Verifies a Nostr event's signature.
		/// </summary>
		/// <param name="nEvent">The event to verify.</param>
		/// <returns>True if the signature is valid; otherwise, false.</returns>
		bool Verify(NEvent nEvent);

		/// <summary>
		/// Sends a Nostr event to relays.
		/// </summary>
		/// <param name="kind">The event kind.</param>
		/// <param name="nEvent">The event to send.</param>
		/// <param name="encryptPubKey">Optional public key for encryption.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task Send(KindEnum kind, NEvent nEvent, string? encryptPubKey = null);

		/// <summary>
		/// Creates a new Nostr event.
		/// </summary>
		/// <param name="kind">The event kind.</param>
		/// <param name="message">The event message content.</param>
		/// <param name="parentId">Optional parent event ID.</param>
		/// <param name="rootId">Optional root event ID.</param>
		/// <param name="ptags">Optional list of public key tags.</param>
		/// <returns>The created Nostr event.</returns>
		NEvent CreateNEvent(KindEnum kind, string message, string? parentId = null, string? rootId = null, List<string>? ptags = null);
	}
}