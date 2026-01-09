using Microsoft.EntityFrameworkCore;
using NostrConnect.Maui.Data;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Services.Persistence;

namespace NostrConnect.Maui.Services
{
    /// <summary>
    /// Implementation of the Nostr data service.
    /// </summary>
    public class NostrDataService : INostrDataService
    {
        private readonly IDbContextFactory<NostrDbContext> _contextFactory;

        public NostrDataService(IDbContextFactory<NostrDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Profile Methods

        public async Task<UserProfile?> GetCurrentUserProfileAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserProfiles
                .FirstOrDefaultAsync(p => p.IsCurrentUser);
        }

        public async Task<List<UserProfile>> GetAllUserProfilesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserProfiles.ToListAsync();
        }

        public async Task<UserProfile?> GetProfileByPublicKeyAsync(string publicKey)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserProfiles
                .FirstOrDefaultAsync(p => p.PublicKey == publicKey);
        }

        public async Task SaveUserProfileAsync(UserProfile profile)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            if (profile.IsCurrentUser)
            {
                var existingCurrent = await context.UserProfiles
                    .Where(p => p.IsCurrentUser && p.PublicKey != profile.PublicKey)
                    .ToListAsync();
                
                foreach (var existing in existingCurrent)
                {
                    existing.IsCurrentUser = false;
                }
                
                // Save current user public key to Preferences for recovery
                Preferences.Default.Set("current_user_pubkey", profile.PublicKey);
            }

            profile.LastUpdated = DateTime.UtcNow;

            // Check if profile already exists in database
            var existingProfile = await context.UserProfiles
                .Include(p => p.Sessions)
                .FirstOrDefaultAsync(p => p.PublicKey == profile.PublicKey);

            if (existingProfile == null)
            {
                // New profile - add it
                context.UserProfiles.Add(profile);
            }
            else
            {
                // Existing profile - update it
                context.Entry(existingProfile).CurrentValues.SetValues(profile);
                
                // Handle session changes
                // Remove sessions that are no longer in the profile
                var sessionsToRemove = existingProfile.Sessions
                    .Where(es => !profile.Sessions.Any(ps => ps.SessionId == es.SessionId))
                    .ToList();
                
                foreach (var session in sessionsToRemove)
                {
                    context.NostrConnectSessions.Remove(session);
                }
                
                foreach (var session in profile.Sessions)
                {
                    var existingSession = existingProfile.Sessions
                        .FirstOrDefault(s => s.SessionId == session.SessionId);
                    
                    if (existingSession == null)
                    {
                        existingProfile.Sessions.Add(session);
                    }
                    else
                    {
                        context.Entry(existingSession).CurrentValues.SetValues(session);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteUserProfileAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var profile = await context.UserProfiles.FindAsync(id);
            if (profile != null)
            {
                context.UserProfiles.Remove(profile);
                await context.SaveChangesAsync();
            }
        }

        #endregion

        #region Relay Methods

        public async Task<List<RelayInfo>> GetAllRelaysAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Relays
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();
        }

        public async Task AddRelayAsync(RelayInfo relay)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var maxOrder = await context.Relays
                .Select(r => (int?)r.DisplayOrder)
                .MaxAsync() ?? 0;
            
            relay.DisplayOrder = maxOrder + 1;
            relay.DateAdded = DateTime.UtcNow;

            context.Relays.Add(relay);
            await context.SaveChangesAsync();
        }

        public async Task UpdateRelayAsync(RelayInfo relay)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Relays.Update(relay);
            await context.SaveChangesAsync();
        }

        public async Task DeleteRelayAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var relay = await context.Relays.FindAsync(id);
            if (relay != null)
            {
                context.Relays.Remove(relay);
                await context.SaveChangesAsync();
            }
        }

        #endregion
    }
}
