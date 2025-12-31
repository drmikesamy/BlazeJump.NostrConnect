using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BlazeJump.Tools.Models
{
    /// <summary>
    /// Represents a Nostr user profile (Kind:0 metadata) stored locally in SQLite.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets the user's public key (hex format), which serves as the primary key.
        /// </summary>
        [Key]
        [JsonProperty("pubkey", NullValueHandling = NullValueHandling.Ignore)]
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's about/bio text.
        /// </summary>
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public string? About { get; set; }

        /// <summary>
        /// Gets or sets the user's profile picture URL.
        /// </summary>
        [JsonProperty("picture", NullValueHandling = NullValueHandling.Ignore)]
        public string? Picture { get; set; }

        /// <summary>
        /// Gets or sets the user's banner image URL.
        /// </summary>
        [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
        public string? Banner { get; set; }

        /// <summary>
        /// Gets or sets the user's website URL.
        /// </summary>
        [JsonIgnore]
        public string? Website { get; set; }

        /// <summary>
        /// Gets or sets the user's NIP-05 identifier (e.g., name@domain.com).
        /// </summary>
        [JsonIgnore]
        public string? Nip05 { get; set; }

        /// <summary>
        /// Gets or sets the user's Lightning Network address.
        /// </summary>
        [JsonIgnore]
        public string? Lud16 { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this profile was last updated.
        /// </summary>
        [JsonIgnore]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether this is the current user's profile.
        /// </summary>
        [JsonIgnore]
        public bool IsCurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the collection of Nostr Connect sessions associated with this user profile.
        /// </summary>
        [JsonIgnore]
        public List<NostrConnect.NostrConnectSession> Sessions { get; set; } = new();
    }
}
