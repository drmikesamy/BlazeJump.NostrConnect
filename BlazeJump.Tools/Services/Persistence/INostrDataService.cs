using BlazeJump.Tools.Models;

namespace BlazeJump.Tools.Services.Persistence
{
    /// <summary>
    /// Service for managing local database operations.
    /// </summary>
    public interface INostrDataService
    {
        Task<UserProfile?> GetCurrentUserProfileAsync();
        Task<List<UserProfile>> GetAllUserProfilesAsync();
        Task<UserProfile?> GetProfileByPublicKeyAsync(string publicKey);
        Task SaveUserProfileAsync(UserProfile profile);
        Task DeleteUserProfileAsync(int id);
        Task<List<RelayInfo>> GetAllRelaysAsync();
        Task AddRelayAsync(RelayInfo relay);
        Task UpdateRelayAsync(RelayInfo relay);
        Task DeleteRelayAsync(int id);
    }
}
