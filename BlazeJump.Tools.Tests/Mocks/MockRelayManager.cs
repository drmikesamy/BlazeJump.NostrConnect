using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Connections.Events;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Tests.Mocks
{
    public class MockRelayManager : IRelayManager
    {
        public event EventHandler ProcessMessageQueue;
        public ConcurrentQueue<NMessage> ReceivedMessages { get; set; } = new();
        public List<string> Relays { get; } = new() { "wss://mock-relay.com" };
        public ConcurrentDictionary<string, IRelayConnection> RelayConnections { get; } = new();

        public Task OpenConnection(string uri)
        {
            return Task.CompletedTask;
        }

        public Task QueryRelays(string subscriptionId, MessageTypeEnum requestMessageType, List<Filter> filters, int timeout = 15000)
        {
            return Task.CompletedTask;
        }

        public Task SendNEvent(NEvent nEvent, string subscriptionHash)
        {
            return Task.CompletedTask;
        }

        public bool TryAddUri(string uri)
        {
            if (!Relays.Contains(uri))
            {
                Relays.Add(uri);
                return true;
            }
            return false;
        }
        
        public Task CloseConnection(string uri) => Task.CompletedTask;
    }
}
