using BlazeJump.Tools.Models;
using NostrConnect.Shared.Models;

namespace NostrConnect.Shared.Services;

public interface INostrService
{
    Task<NostrKeyPair> GenerateKeyPairAsync();
    Task<NEvent> CreateEventAsync(int kind, string content, List<EventTag>? tags = null);
    Task<NEvent> SignEventAsync(NEvent nEvent, NostrKeyPair keyPair);
    Task PublishEventAsync(NEvent nEvent, string relayUrl);
    Task<List<NEvent>> SubscribeToEventsAsync(string relayUrl, Filter filter);
    string GetPublicKeyHex(NostrKeyPair keyPair);
}
