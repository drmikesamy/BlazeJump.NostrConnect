using Microsoft.EntityFrameworkCore;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Models.NostrConnect;
using Newtonsoft.Json;
using NostrConnect.Maui.Models;

namespace NostrConnect.Maui.Data
{
    /// <summary>
    /// Database context for the NostrConnect Maui application.
    /// </summary>
    public class NostrDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the user profiles table.
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }

        /// <summary>
        /// Gets or sets the relay information table.
        /// </summary>
        public DbSet<RelayInfo> Relays { get; set; }

        /// <summary>
        /// Gets or sets the Nostr Connect sessions table.
        /// </summary>
        public DbSet<NostrConnectSession> NostrConnectSessions { get; set; }

        /// <summary>
        /// Gets or sets the health data table.
        /// </summary>
        public DbSet<HealthData> HealthData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NostrDbContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public NostrDbContext(DbContextOptions<NostrDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasIndex(e => e.PublicKey).IsUnique();
                entity.HasIndex(e => e.IsCurrentUser);
            });

            // Configure RelayInfo
            modelBuilder.Entity<RelayInfo>(entity =>
            {
                entity.HasIndex(e => e.Url).IsUnique();
                entity.HasIndex(e => e.DisplayOrder);
            });

            // Configure NostrConnectSession
            modelBuilder.Entity<NostrConnectSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.HasIndex(e => e.OurPubkey);
                entity.HasIndex(e => e.TheirPubkey).IsUnique();
                
                // Enforce required relationship to UserProfile
                entity.HasOne<UserProfile>()
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(e => e.OurPubkey)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure conversions for lists
                entity.Property(e => e.Relays)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());
                
                entity.Property(e => e.Permissions)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());
            });

            // Configure HealthData
            modelBuilder.Entity<HealthData>(entity =>
            {
                entity.HasIndex(e => e.PublicKey);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Timestamp);
            });
        }
    }
}
