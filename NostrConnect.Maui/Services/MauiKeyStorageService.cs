using NostrConnect.Shared.Models;
using NostrConnect.Shared.Services;
using Newtonsoft.Json;

namespace NostrConnect.Maui.Services;

public class MauiKeyStorageService : IKeyStorageService
{
    private const string KeyPairKey = "nostr_keypair";

    public async Task<NostrKeyPair?> GetStoredKeyPairAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(KeyPairKey);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonConvert.DeserializeObject<NostrKeyPair>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveKeyPairAsync(NostrKeyPair keyPair)
    {
        var json = JsonConvert.SerializeObject(keyPair);
        await SecureStorage.SetAsync(KeyPairKey, json);
    }

    public async Task<bool> HasStoredKeyPairAsync()
    {
        var keyPair = await GetStoredKeyPairAsync();
        return keyPair != null;
    }

    public async Task ClearKeyPairAsync()
    {
        SecureStorage.Remove(KeyPairKey);
        await Task.CompletedTask;
    }
}
