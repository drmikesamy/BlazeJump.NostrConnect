using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections.Events;
using BlazeJump.Tools.Enums;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Connections
{
	/// <summary>
	/// Interface for managing connections to multiple Nostr relays.
	/// </summary>
	public interface IRelayManager
	{
		/// <summary>
		/// Event raised when messages need to be processed from the queue.
		/// </summary>
		event EventHandler ProcessMessageQueue;

		/// <summary>
		/// Gets or sets the queue of received messages from all relays.
		/// </summary>
		ConcurrentQueue<NMessage> ReceivedMessages { get; set; }

		/// <summary>
		/// Gets the list of relay URIs being managed.
		/// </summary>
		List<string> Relays { get; }

		/// <summary>
		/// Gets or sets the dictionary of active relay connections.
		/// </summary>
		ConcurrentDictionary<string, IRelayConnection> RelayConnections { get; }

		/// <summary>
		/// Attempts to add a relay URI to the manager without opening a connection.
		/// </summary>
		/// <param name="uri">The relay URI to add.</param>
		/// <returns>True if the URI was added successfully; false if it already exists.</returns>
		bool TryAddUri(string uri);

		/// <summary>
		/// Opens a connection to a relay.
		/// </summary>
		/// <param name="uri">The relay URI.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task OpenConnection(string uri);

		/// <summary>
		/// Closes a connection to a relay.
		/// </summary>
		/// <param name="uri">The relay URI.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task CloseConnection(string uri);

		/// <summary>
		/// Queries multiple relays with filters.
		/// </summary>
		/// <param name="subscriptionId">The subscription identifier.</param>
		/// <param name="requestMessageType">The type of request message.</param>
		/// <param name="filters">The list of filters.</param>
		/// <param name="timeout">The timeout in milliseconds (default 15000).</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000);

		/// <summary>
		/// Sends a Nostr event to all connected relays.
		/// </summary>
		/// <param name="nEvent">The Nostr event to send.</param>
		/// <param name="subscriptionHash">The subscription hash.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SendNEvent(NEvent nEvent, string subscriptionHash);
	}
}
