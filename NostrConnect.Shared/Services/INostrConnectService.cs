using NostrConnect.Shared.Models;

namespace NostrConnect.Shared.Services;

public interface INostrConnectService
{
    Task<NostrConnectSession> CreateSessionAsync(string webEphemeralPubKey);
    Task<NostrConnectSession?> GetSessionAsync(string sessionId);
    Task<bool> ConnectSessionAsync(string webPubKey, string appPubKey, string? relay = null, string? secret = null);
    Task<NostrConnectResponse> HandleRequestAsync(NostrConnectRequest request);
}
