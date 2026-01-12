using Hl7.Fhir.Model;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Medication resources to simplify UI binding.
/// </summary>
public static class MedicationExtensions
{
    /// <summary>
    /// Gets the medication name.
    /// </summary>
    public static string GetName(this Medication medication)
    {
        return medication.Code?.Text ?? 
               medication.Code?.Coding?.FirstOrDefault()?.Display ?? 
               "Unknown Medication";
    }

    /// <summary>
    /// Gets the dosage instruction.
    /// </summary>
    public static string GetDosage(this Medication medication)
    {
        // Dosage is typically stored in MedicationRequest, not Medication
        // For now, we'll store it in extension or use Code.Text with format "Name - Dosage"
        var text = medication.Code?.Text ?? string.Empty;
        if (text.Contains(" - "))
        {
            var parts = text.Split(" - ");
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets the frequency instruction.
    /// </summary>
    public static string GetFrequency(this Medication medication)
    {
        // Frequency is stored in extension for now
        var extension = medication.Extension?.FirstOrDefault(e => e.Url == "frequency");
        return extension?.Value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public static DateTime GetStartDate(this Medication medication)
    {
        var extension = medication.Extension?.FirstOrDefault(e => e.Url == "startDate");
        if (extension?.Value is FhirDateTime dateTime)
        {
            return dateTime.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        }
        return medication.Meta?.LastUpdated?.DateTime ?? DateTime.MinValue;
    }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public static DateTime? GetEndDate(this Medication medication)
    {
        var extension = medication.Extension?.FirstOrDefault(e => e.Url == "endDate");
        if (extension?.Value is FhirDateTime dateTime)
        {
            return dateTime.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        }
        return null;
    }

    /// <summary>
    /// Checks if the medication is active.
    /// </summary>
    public static bool IsActive(this Medication medication)
    {
        var extension = medication.Extension?.FirstOrDefault(e => e.Url == "isActive");
        if (extension?.Value is FhirBoolean boolValue)
        {
            return boolValue.Value ?? true;
        }
        // Check if there's an end date - if not, it's active
        return GetEndDate(medication) == null;
    }

    /// <summary>
    /// Sets the medication name.
    /// </summary>
    public static void SetName(this Medication medication, string name)
    {
        medication.Code ??= new CodeableConcept();
        
        // If there's existing dosage info, preserve it
        var existingDosage = medication.GetDosage();
        if (!string.IsNullOrEmpty(existingDosage))
        {
            medication.Code.Text = $"{name} - {existingDosage}";
        }
        else
        {
            medication.Code.Text = name;
        }
    }

    /// <summary>
    /// Sets the dosage instruction.
    /// </summary>
    public static void SetDosage(this Medication medication, string dosage)
    {
        medication.Code ??= new CodeableConcept();
        
        var name = medication.GetName();
        if (name.Contains(" - "))
        {
            name = name.Split(" - ")[0];
        }
        
        medication.Code.Text = $"{name} - {dosage}";
    }

    /// <summary>
    /// Sets the frequency instruction.
    /// </summary>
    public static void SetFrequency(this Medication medication, string frequency)
    {
        medication.Extension ??= new List<Extension>();
        
        var existing = medication.Extension.FirstOrDefault(e => e.Url == "frequency");
        if (existing != null)
        {
            medication.Extension.Remove(existing);
        }
        
        medication.Extension.Add(new Extension
        {
            Url = "frequency",
            Value = new FhirString(frequency)
        });
    }

    /// <summary>
    /// Sets the start date.
    /// </summary>
    public static void SetStartDate(this Medication medication, DateTime startDate)
    {
        medication.Extension ??= new List<Extension>();
        
        var existing = medication.Extension.FirstOrDefault(e => e.Url == "startDate");
        if (existing != null)
        {
            medication.Extension.Remove(existing);
        }
        
        medication.Extension.Add(new Extension
        {
            Url = "startDate",
            Value = new FhirDateTime(startDate)
        });
    }

    /// <summary>
    /// Sets the end date.
    /// </summary>
    public static void SetEndDate(this Medication medication, DateTime? endDate)
    {
        medication.Extension ??= new List<Extension>();
        
        var existing = medication.Extension.FirstOrDefault(e => e.Url == "endDate");
        if (existing != null)
        {
            medication.Extension.Remove(existing);
        }
        
        if (endDate.HasValue)
        {
            medication.Extension.Add(new Extension
            {
                Url = "endDate",
                Value = new FhirDateTime(endDate.Value)
            });
        }
    }

    /// <summary>
    /// Sets whether the medication is active.
    /// </summary>
    public static void SetIsActive(this Medication medication, bool isActive)
    {
        medication.Extension ??= new List<Extension>();
        
        var existing = medication.Extension.FirstOrDefault(e => e.Url == "isActive");
        if (existing != null)
        {
            medication.Extension.Remove(existing);
        }
        
        medication.Extension.Add(new Extension
        {
            Url = "isActive",
            Value = new FhirBoolean(isActive)
        });
    }

    /// <summary>
    /// Creates a new FHIR Medication.
    /// </summary>
    public static Medication CreateNew(
        string name,
        string dosage,
        string frequency,
        DateTime startDate,
        bool isActive = true)
    {
        var medication = new Medication
        {
            Id = Guid.NewGuid().ToString(),
            Status = Medication.MedicationStatusCodes.Active,
            Code = new CodeableConcept
            {
                Text = $"{name} - {dosage}"
            },
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow
            }
        };

        medication.SetFrequency(frequency);
        medication.SetStartDate(startDate);
        medication.SetIsActive(isActive);

        return medication;
    }
}
