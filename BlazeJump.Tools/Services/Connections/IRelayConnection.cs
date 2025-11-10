using System.Collections.Concurrent;
using System.Net.WebSockets;
using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections.Events;

namespace BlazeJump.Tools.Services.Connections;

/// <summary>
/// Interface for managing a connection to a single Nostr relay.
/// </summary>
public interface IRelayConnection
{
	/// <summary>
	/// Gets a value indicating whether the relay connection is open.
	/// </summary>
    bool IsOpen { get; }
	
	/// <summary>
	/// Gets or sets the dictionary of active subscriptions (subscription ID to active status).
	/// </summary>
	ConcurrentDictionary<string, bool> ActiveSubscriptions { get; set; }
	
	/// <summary>
	/// Event raised when a new message is received from the relay.
	/// </summary>
    event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
	
	/// <summary>
	/// Initializes and opens the connection to the relay.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
    Task Init();
	
	/// <summary>
	/// Subscribes to events from the relay with the specified filters.
	/// </summary>
	/// <param name="requestMessageType">The type of request message.</param>
	/// <param name="subscriptionId">The unique subscription identifier.</param>
	/// <param name="filters">The list of filters for the subscription.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeAsync(MessageTypeEnum requestMessageType, string subscriptionId, List<Filter> filters);
	
	/// <summary>
	/// Sends a Nostr event to the relay.
	/// </summary>
	/// <param name="serialisedNMessage">The serialized Nostr message.</param>
	/// <param name="subscriptionId">The subscription identifier.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
    Task SendNEvent(string serialisedNMessage, string subscriptionId);
	
	/// <summary>
	/// Closes the connection to the relay.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
    Task Close();
}