using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections.Events;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using BlazeJump.Tools.Services.Connections.Factories;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Services.Connections
{
	/// <summary>
	/// Manages a WebSocket connection to a single Nostr relay.
	/// </summary>
    public class RelayConnection : IRelayConnection
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly string _uri;
        private IClientWebSocketWrapper _webSocket { get; set; }
        private IClientWebSocketFactory _webSocketFactory { get; set; }
		
		/// <summary>
		/// Gets or sets the dictionary of active subscriptions.
		/// </summary>
        public ConcurrentDictionary<string, bool> ActiveSubscriptions { get; set; } = new ConcurrentDictionary<string, bool>();

		/// <summary>
		/// Event raised when a new message is received from the relay.
		/// </summary>
		public event EventHandler<MessageReceivedEventArgs>? NewMessageReceived;
		
		/// <summary>
		/// Gets a value indicating whether the connection is open.
		/// </summary>
        public bool IsOpen => _webSocket is { State: WebSocketState.Open };

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayConnection"/> class.
		/// </summary>
		/// <param name="webSocketFactory">The WebSocket factory.</param>
		/// <param name="uri">The relay URI.</param>
        public RelayConnection(IClientWebSocketFactory webSocketFactory, string uri)
        {
            _webSocketFactory = webSocketFactory;
            _webSocket = _webSocketFactory.Create();
            _uri = uri;
        }

		/// <summary>
		/// Initializes and opens the connection to the relay.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
        public async Task Init()
        {
            await ConnectAsync();
            _ = MessageLoop();
        }

        private async Task ConnectAsync()
        {
            if (IsOpen)
            {
                Console.WriteLine($"Already connected to relay: {_uri}.");
                return;
            }
            else if (_webSocket.State == WebSocketState.Aborted)
            {
                _webSocket.Dispose();
            }

            _webSocket = _webSocketFactory.Create();
            await _webSocket.ConnectAsync(new Uri(_uri), _cancellationTokenSource.Token);
        }

		/// <summary>
		/// Subscribes to events from the relay with the specified filters.
		/// </summary>
		/// <param name="requestMessageType">The type of request message.</param>
		/// <param name="subscriptionId">The unique subscription identifier.</param>
		/// <param name="filters">The list of filters for the subscription.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
        public async Task SubscribeAsync(MessageTypeEnum requestMessageType, string subscriptionId,
            List<Filter> filters)
        {
            if (IsOpen)
            {
                if (ActiveSubscriptions.ContainsKey(subscriptionId))
                {
                    Console.WriteLine($"Already subscribed to {_uri} using {subscriptionId}");
                }
                else
                {
                    Console.WriteLine($"Subscribing to {_uri} using {subscriptionId}");
                    await SendRequest(requestMessageType, subscriptionId, filters);
                    ActiveSubscriptions.TryAdd(subscriptionId, true);
				}
            }
        }

        private async Task SendRequest(MessageTypeEnum requestMessageType, string subscriptionId, List<Filter> filters)
        {
            object[] obj = new object[2 + filters.Count];
            obj[0] = requestMessageType.ToString().ToUpper();
            obj[1] = subscriptionId;
            for (var i = 2; i < 2 + filters.Count; i++)
            {
                obj[i] = filters[i - 2];
            }

            string newsub = JsonConvert.SerializeObject(obj);

            var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(newsub));

            await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        private async Task MessageLoop()
        {
            var messages = new List<string>();
            Console.WriteLine($"setting up listener for {_uri}");
            await foreach (var message in ReceiveLoop())
            {
                NewMessageReceived?.Invoke(this, new MessageReceivedEventArgs(_uri, message));
            }
        }

        private async IAsyncEnumerable<NMessage> ReceiveLoop()
        {
            var canceled = false;
            var buffer = new ArraySegment<byte>(new byte[2048]);
            while (true)
            {
                WebSocketReceiveResult result;
                using var ms = new MemoryStream();
                try
                {
                    do
                    {
                        result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                        ms.Write(buffer.Array!, buffer.Offset, result.Count);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            canceled = true;
                        }
                    } while (!result.EndOfMessage);
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                }

                ms.Seek(0, SeekOrigin.Begin);
                var rawMessage = Encoding.UTF8.GetString(ms.ToArray());
                if (string.IsNullOrEmpty(rawMessage))
                {
                    continue;
                }

                var message = JsonConvert.DeserializeObject<NMessage>(rawMessage);
                if (message != null)
                {
                    Console.WriteLine($"Message received from {_uri}: {message.SubscriptionId} on thread {Thread.CurrentThread.ManagedThreadId}");
                    yield return message;
                    if (canceled && message.SubscriptionId != null)
                    {
                        Console.WriteLine($"Cancelled, unsubscribing from {_uri}");
                        await UnSubscribeAsync(message.SubscriptionId);
                        break;
                    }
                }
            }
        }

		/// <summary>
		/// Sends a Nostr event to the relay.
		/// </summary>
		/// <param name="serialisedNMessage">The serialized Nostr message.</param>
		/// <param name="subscriptionId">The subscription identifier.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendNEvent(string serialisedNMessage, string subscriptionId)
        {
            if (IsOpen)
            {
                Console.WriteLine($"Sending NEvent to {_uri} using {subscriptionId}");

                var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialisedNMessage));

                await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        private async Task UnSubscribeAsync(string subscriptionId)
        {
            if (IsOpen)
            {
                Console.WriteLine($"Unsubscribing from {_uri}, subscriptionid {subscriptionId}");
                object[] obj = { "CLOSE", subscriptionId };

                string closeMessage = JsonConvert.SerializeObject(obj);

                var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(closeMessage));

                await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
				ActiveSubscriptions.TryRemove(subscriptionId, out _);
			}
        }

		/// <summary>
		/// Closes the connection to the relay.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
        public async Task Close()
        {
            var unsubscribeTasks = new List<Task>();

            foreach (var activeSubscription in ActiveSubscriptions)
            {
                Task connectionTask = Task.Run(async () => { await UnSubscribeAsync(activeSubscription.Key); });
                unsubscribeTasks.Add(connectionTask);
            }

            await Task.WhenAll(unsubscribeTasks);

            _cancellationTokenSource.Cancel();
            _webSocket.Dispose();
        }
    }
}