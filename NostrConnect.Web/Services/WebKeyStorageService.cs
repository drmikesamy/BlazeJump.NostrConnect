using NostrConnect.Shared.Models;
using NostrConnect.Shared.Services;

namespace NostrConnect.Web.Services;

public class WebKeyStorageService : IKeyStorageService
{
    private NostrKeyPair? _ephemeralKeyPair;

    public async Task<NostrKeyPair?> GetStoredKeyPairAsync()
    {
        return await Task.FromResult(_ephemeralKeyPair);
    }

    public async Task SaveKeyPairAsync(NostrKeyPair keyPair)
    {
        _ephemeralKeyPair = keyPair;
        await Task.CompletedTask;
    }

    public async Task<bool> HasStoredKeyPairAsync()
    {
        return await Task.FromResult(_ephemeralKeyPair != null);
    }

    public async Task ClearKeyPairAsync()
    {
        _ephemeralKeyPair = null;
        await Task.CompletedTask;
    }
}
