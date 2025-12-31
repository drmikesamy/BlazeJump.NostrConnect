using BlazeJump.Tools.Enums;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Message;
using System.Collections.Concurrent;

namespace BlazeJump.Tools.Tests.Mocks
{
    public class LoopbackMessageService : IMessageService
    {
        public event EventHandler<NMessage>? ProcessNostrConnectMessage;
        
        public RelationRegister RelationRegister { get; set; } = new();
        public ConcurrentDictionary<string, NMessage> MessageStore { get; set; } = new();

        public NEvent CreateNEvent(string pubkey, KindEnum kind, string message, string? parentId = null, string? rootId = null, List<string>? ptags = null)
        {
            var nEvent = new NEvent
            {
                Id = Guid.NewGuid().ToString(),
                Pubkey = pubkey,
                Kind = kind,
                Content = message,
                Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Tags = new List<EventTag>()
            };

            if (ptags != null)
            {
                foreach (var tag in ptags)
                {
                    nEvent.Tags.Add(new EventTag { Key = TagEnum.p, Value = tag });
                }
            }
            
            return nEvent;
        }

        public Task Send(KindEnum kind, NEvent nEvent, string? encryptPubKey = null)
        {
            // Route directly to listeners
            var message = new NMessage
            {
                MessageType = MessageTypeEnum.Event,
                SubscriptionId = "loopback",
                Event = nEvent
            };

            // Using Task.Run to simulate async decoupling and avoid deadlocks if calls are blocking
            _ = Task.Run(() => 
            {
                try 
                {
                    ProcessNostrConnectMessage?.Invoke(this, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Loopback dispatch error: {ex}");
                }
            });

            return Task.CompletedTask;
        }

        public Task Fetch(List<Filter> filters, string? subscriptionId = null, MessageTypeEnum? messageType = null) => Task.CompletedTask;
        public Task FetchPage(string hex, DateTime? untilMarker = null) => Task.CompletedTask;
        public Task LookupUser(string searchString) => Task.CompletedTask;
        public bool Verify(NEvent nEvent) => true;

        // Extra debug helper
        public List<NMessage> History { get; } = new();
    }
}
