using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.Crypto;
using BlazeJump.Tools.Services.Connections;
using NostrConnect.Shared.Models;
using BlazeJump.Tools.Builders;
using BlazeJump.Tools.Enums;

namespace NostrConnect.Shared.Services;

public class NostrService : INostrService
{
    private readonly ICryptoService _cryptoService;
    private readonly IRelayManager _relayManager;

    public NostrService(ICryptoService cryptoService, IRelayManager relayManager)
    {
        _cryptoService = cryptoService;
        _relayManager = relayManager;
    }

    public async Task<NostrKeyPair> GenerateKeyPairAsync()
    {
        var keyPair = _cryptoService.GetNewSecp256k1KeyPair();
        return await Task.FromResult(new NostrKeyPair
        {
            PublicKey = Convert.ToHexString(keyPair.PublicKey.ToXOnlyPubKey().ToBytes()).ToLower(),
            PrivateKey = Convert.ToHexString(keyPair.PrivateKey.sec.ToBytes()).ToLower(),
            IsEphemeral = false
        });
    }

    public async Task<NEvent> CreateEventAsync(int kind, string content, List<EventTag>? tags = null)
    {
        return await Task.FromResult(new NEvent
        {
            Kind = (KindEnum)kind,
            Content = content,
            Tags = tags ?? new List<EventTag>(),
            Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    public async Task<NEvent> SignEventAsync(NEvent nEvent, NostrKeyPair keyPair)
    {
        // Ensure the crypto service has the permanent key loaded
        await _cryptoService.CreateOrLoadPermanentKeyPair();
        
        // Serialize event for ID generation (NIP-01)
        // Event ID = SHA256 hash of serialized event data
        var serialized = System.Text.Json.JsonSerializer.Serialize(new object[]
        {
            0, // Reserved for future use
            keyPair.PublicKey,
            nEvent.Created_At,
            (int)(nEvent.Kind ?? KindEnum.Text),
            nEvent.Tags ?? new List<EventTag>(),
            nEvent.Content ?? string.Empty
        });
        
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(serialized));
        var eventId = Convert.ToHexString(hash).ToLower();
        
        // Sign the event ID (use permanent key, not ethereal)
        var signature = await _cryptoService.Sign(eventId, ethereal: false);
        
        var signedEvent = new NEvent
        {
            Id = eventId,
            Pubkey = keyPair.PublicKey,
            Created_At = nEvent.Created_At,
            Kind = nEvent.Kind,
            Tags = nEvent.Tags,
            Content = nEvent.Content,
            Sig = signature
        };
        
        return signedEvent;
    }

    public async Task PublishEventAsync(NEvent nEvent, string relayUrl)
    {
        // In production, properly connect and publish to relay
        await Task.CompletedTask;
    }

    public async Task<List<NEvent>> SubscribeToEventsAsync(string relayUrl, Filter filter)
    {
        var events = new List<NEvent>();
        // In production, connect to relay and subscribe
        return await Task.FromResult(events);
    }

    public string GetPublicKeyHex(NostrKeyPair keyPair)
    {
        return keyPair.PublicKey;
    }
}
