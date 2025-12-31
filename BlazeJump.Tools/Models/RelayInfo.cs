using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazeJump.Tools.Models;

    /// <summary>
    /// Represents a Nostr relay configuration stored locally in SQLite.
    /// </summary>
    public class RelayInfo
    {
        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the relay URL (e.g., wss://relay.damus.io).
        /// </summary>
        [Required]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this relay is enabled for reading events.
        /// </summary>
        public bool IsReadEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this relay is enabled for writing events.
        /// </summary>
        public bool IsWriteEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the timestamp when this relay was added.
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when this relay was last used successfully.
        /// </summary>
        public DateTime? LastConnected { get; set; }

        /// <summary>
        /// Gets or sets the display order for the relay list.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
