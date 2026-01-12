using System.ComponentModel.DataAnnotations;

namespace NostrConnect.Maui.Models;

/// <summary>
/// Represents a local FHIR resource stored in the database.
/// </summary>
public class LocalResource
{
    /// <summary>
    /// Gets or sets the unique identifier for the resource.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the FHIR resource type (e.g., "Observation", "Medication", "Appointment").
    /// </summary>
    public required string FhirType { get; set; }

    /// <summary>
    /// Gets or sets the FHIR resource content as a JSON string.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the last updated timestamp.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the Nostr event ID if the resource has been backed up to Nostr.
    /// </summary>
    public string? NostrEventId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the resource is soft-deleted.
    /// Used to sync deletions to Nostr.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
