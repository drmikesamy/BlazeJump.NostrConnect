using System.ComponentModel.DataAnnotations;

namespace NostrConnect.Maui.Models;

public class HealthData
{
    [Key]
    public int Id { get; set; }
    public required string PublicKey { get; set; }
    public required string Type { get; set; }
    public string? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string? NostrEventId { get; set; }
}

public class VitalSign
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class Medication
{
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class Appointment
{
    public string Title { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
