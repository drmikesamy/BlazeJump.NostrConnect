using NostrConnect.Shared.Models;

namespace NostrConnect.Shared.Services;

public interface IKeyStorageService
{
    Task<NostrKeyPair?> GetStoredKeyPairAsync();
    Task SaveKeyPairAsync(NostrKeyPair keyPair);
    Task<bool> HasStoredKeyPairAsync();
    Task ClearKeyPairAsync();
}
