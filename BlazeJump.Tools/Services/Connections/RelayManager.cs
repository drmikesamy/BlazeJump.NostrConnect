using BlazeJump.Tools.Models;
using Newtonsoft.Json;
using BlazeJump.Tools.Services.Connections.Events;
using BlazeJump.Tools.Enums;
using System.Diagnostics;
using BlazeJump.Tools.Services.Connections.Providers;
using System.Threading;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Connections
{
	/// <summary>
	/// Manages connections to multiple Nostr relays and coordinates message distribution.
	/// </summary>
	public class RelayManager : IRelayManager
	{
		/// <summary>
		/// Gets or sets the dictionary of active relay connections.
		/// </summary>
		public ConcurrentDictionary<string, IRelayConnection> RelayConnections { get; private set; } = new ConcurrentDictionary<string, IRelayConnection>();
		
		/// <summary>
		/// Gets the list of relay URIs being managed.
		/// </summary>
		public List<string> Relays => RelayConnections.Keys.ToList();
		
		/// <summary>
		/// Gets or sets the queue of received messages from all relays.
		/// </summary>
		public ConcurrentQueue<NMessage> ReceivedMessages { get; set; } = new ConcurrentQueue<NMessage>();
		
		/// <summary>
		/// Event raised when messages need to be processed from the queue.
		/// </summary>
		public event EventHandler? ProcessMessageQueue;
		private readonly IRelayConnectionProvider _connectionProvider;
		private readonly object _processMessageQueueLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayManager"/> class.
		/// </summary>
		/// <param name="connectionProvider">The relay connection provider.</param>
		public RelayManager(IRelayConnectionProvider connectionProvider)
		{
			_connectionProvider = connectionProvider;	
		}

		/// <summary>
		/// Attempts to add a relay URI to the manager without opening a connection.
		/// </summary>
		/// <param name="uri">The relay URI to add.</param>
		/// <returns>True if the URI was added successfully; false if it already exists.</returns>
		public bool TryAddUri(string uri)
		{
			return RelayConnections.TryAdd(uri, _connectionProvider.CreateRelayConnection(uri));
		}

		private void AddToQueue(object? sender, MessageReceivedEventArgs e)
		{
			Console.WriteLine($"Adding Event {e.Message?.Event?.Id} to queue");
			if (e.Message != null)
			{
				ReceivedMessages.Enqueue(e.Message);
			}
			lock (_processMessageQueueLock)
			{
				ProcessMessageQueue?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Opens a connection to a relay.
		/// </summary>
		/// <param name="uri">The relay URI.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task OpenConnection(string uri)
		{
			if (RelayConnections.TryGetValue(uri, out IRelayConnection? connection) && connection.IsOpen)
			{
				return;
			}
			IRelayConnection newConnection = _connectionProvider.CreateRelayConnection(uri);
			RelayConnections.TryAdd(uri, newConnection);
			await RelayConnections[uri].Init();
			Console.WriteLine("Event subscription");
			RelayConnections[uri].NewMessageReceived += AddToQueue;
		}

		/// <summary>
		/// Closes a connection to a relay.
		/// </summary>
		/// <param name="uri">The relay URI.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task CloseConnection(string uri)
		{
			if (RelayConnections.TryRemove(uri, out var connection) && connection != null)
			{
				await connection.Close();
			}
		}

		/// <summary>
		/// Queries multiple relays with filters.
		/// </summary>
		/// <param name="subscriptionId">The subscription identifier.</param>
		/// <param name="requestMessageType">The type of request message.</param>
		/// <param name="filters">The list of filters.</param>
		/// <param name="timeout">The timeout in milliseconds.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000)
		{
			using var resource = new SemaphoreSlim(5, 5);

			var connectionTasks = Relays.Select(async uri =>
			{
				await resource.WaitAsync(timeout);
				try
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SubscribeAsync(requestMessageType, subscriptionId, filters);
				}
				catch
				{
					Console.WriteLine($"Failed to connect to relay {uri}");
				}
				finally
				{
					resource.Release();
				}
			});
			await Task.WhenAll(connectionTasks);
		}

		/// <summary>
		/// Sends a Nostr event to all relays.
		/// </summary>
		/// <param name="nEvent">The Nostr event to send.</param>
		/// <param name="subscriptionHash">The subscription hash.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task SendNEvent(NEvent nEvent, string subscriptionHash)
		{
			using var resource = new SemaphoreSlim(5, 5);

			object[] obj = { "EVENT", nEvent };
			string serialisedNEvent = JsonConvert.SerializeObject(obj);

			var connectionTasks = Relays.Select(async uri =>
			{
				await resource.WaitAsync();
				try
				{
					await OpenConnection(uri);
					await RelayConnections[uri].SendNEvent(serialisedNEvent, subscriptionHash);
				}
				catch
				{
					Console.WriteLine($"Failed to connect to relay {uri}");
				}
				finally
				{
					resource.Release();
				}
			});
			await Task.WhenAll(connectionTasks);
		}
	}
}
