using BlazeJump.Tools.Services.Persistence;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;

namespace BlazeJump.Tools.Tests.Mocks
{
    public class MockNostrDataService : INostrDataService
    {
        public List<UserProfile> Profiles { get; set; } = new List<UserProfile>();
        public List<RelayInfo> Relays { get; set; } = new List<RelayInfo>();

        public Task<UserProfile?> GetCurrentUserProfileAsync()
        {
            return Task.FromResult(Profiles.FirstOrDefault(p => p.IsCurrentUser));
        }

        public Task<List<UserProfile>> GetAllUserProfilesAsync()
        {
            return Task.FromResult(Profiles);
        }

        public Task<UserProfile?> GetProfileByPublicKeyAsync(string publicKey)
        {
            return Task.FromResult(Profiles.FirstOrDefault(p => p.PublicKey == publicKey));
        }

        public Task SaveUserProfileAsync(UserProfile profile)
        {
            var existing = Profiles.FirstOrDefault(p => p.PublicKey == profile.PublicKey);
            if (existing != null)
            {
                Profiles.Remove(existing);
            }
            Profiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task DeleteUserProfileAsync(int id)
        {
            // Note: In mock we mimic behavior without precise ID handling unless we enforce it
            // Assuming ID isn't critical for these tests or finding by other means
            return Task.CompletedTask; 
        }

        public Task<List<RelayInfo>> GetAllRelaysAsync()
        {
            return Task.FromResult(Relays);
        }

        public Task AddRelayAsync(RelayInfo relay)
        {
            Relays.Add(relay);
            return Task.CompletedTask;
        }

        public Task UpdateRelayAsync(RelayInfo relay)
        {
            // Reference type, already updated in list effectively
            return Task.CompletedTask;
        }

        public Task DeleteRelayAsync(int id)
        {
            var relay = Relays.FirstOrDefault(r => r.Id == id);
            if (relay != null)
                Relays.Remove(relay);
            return Task.CompletedTask;
        }
    }
}
