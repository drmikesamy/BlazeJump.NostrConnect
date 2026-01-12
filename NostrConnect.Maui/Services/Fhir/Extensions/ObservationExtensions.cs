using Hl7.Fhir.Model;
using MudBlazor;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Observation resources (Vital Signs) to simplify UI binding.
/// </summary>
public static class ObservationExtensions
{
    /// <summary>
    /// Gets the observation type/name (e.g., "Heart Rate", "Blood Pressure").
    /// </summary>
    public static string GetType(this Observation observation)
    {
        return observation.Code?.Text ?? observation.Code?.Coding?.FirstOrDefault()?.Display ?? "Unknown";
    }

    /// <summary>
    /// Gets the numeric value from the observation.
    /// </summary>
    public static decimal GetValue(this Observation observation)
    {
        if (observation.Value is Quantity quantity)
        {
            return quantity.Value ?? 0;
        }
        return 0;
    }

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    public static string GetUnit(this Observation observation)
    {
        if (observation.Value is Quantity quantity)
        {
            return quantity.Unit ?? quantity.Code ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets the observation timestamp.
    /// </summary>
    public static DateTime GetTimestamp(this Observation observation)
    {
        if (observation.Effective is FhirDateTime effectiveDateTime)
        {
            return effectiveDateTime.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        }
        if (observation.Effective is Period period && period.Start != null)
        {
            return DateTime.Parse(period.Start);
        }
        return observation.Issued?.DateTime ?? DateTime.MinValue;
    }

    /// <summary>
    /// Sets the observation type.
    /// </summary>
    public static void SetType(this Observation observation, string type)
    {
        observation.Code ??= new CodeableConcept();
        observation.Code.Text = type;
    }

    /// <summary>
    /// Sets the observation value and unit.
    /// </summary>
    public static void SetValue(this Observation observation, decimal value, string unit)
    {
        observation.Value = new Quantity
        {
            Value = value,
            Unit = unit,
            System = "http://unitsofmeasure.org"
        };
    }

    /// <summary>
    /// Sets the observation timestamp.
    /// </summary>
    public static void SetTimestamp(this Observation observation, DateTime timestamp)
    {
        observation.Effective = new FhirDateTime(timestamp);
        observation.Issued = new DateTimeOffset(timestamp);
    }

    /// <summary>
    /// Creates a new FHIR Observation for a vital sign.
    /// </summary>
    public static Observation CreateVitalSign(
        string type,
        decimal value,
        string unit,
        string? patientId = null)
    {
        var observation = new Observation
        {
            Id = Guid.NewGuid().ToString(),
            Status = ObservationStatus.Final,
            Category = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://terminology.hl7.org/CodeSystem/observation-category",
                            Code = "vital-signs",
                            Display = "Vital Signs"
                        }
                    }
                }
            },
            Code = new CodeableConcept
            {
                Text = type
            },
            Value = new Quantity
            {
                Value = value,
                Unit = unit,
                System = "http://unitsofmeasure.org"
            },
            Effective = new FhirDateTime(DateTime.UtcNow),
            Issued = DateTimeOffset.UtcNow,
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow
            }
        };

        if (!string.IsNullOrEmpty(patientId))
        {
            observation.Subject = new ResourceReference
            {
                Reference = $"Patient/{patientId}"
            };
        }

        return observation;
    }

    /// <summary>
    /// Gets the icon name based on vital sign type.
    /// </summary>
    public static string GetVitalIcon(this Observation observation)
    {
        var typeName = observation.Code?.Text ?? string.Empty;
        var typeLower = typeName.ToLowerInvariant();
        
        if (typeLower.Contains("heart") || typeLower.Contains("pulse"))
            return Icons.Material.Filled.Favorite;
        if (typeLower.Contains("blood pressure") || typeLower.Contains("bp"))
            return Icons.Material.Filled.Bloodtype;
        if (typeLower.Contains("temperature") || typeLower.Contains("temp"))
            return Icons.Material.Filled.Thermostat;
        if (typeLower.Contains("weight"))
            return Icons.Material.Filled.FitnessCenter;
        if (typeLower.Contains("glucose") || typeLower.Contains("sugar"))
            return Icons.Material.Filled.Opacity;
        if (typeLower.Contains("oxygen") || typeLower.Contains("spo2"))
            return Icons.Material.Filled.Air;
        
        return Icons.Material.Filled.MonitorHeart;
    }
}
