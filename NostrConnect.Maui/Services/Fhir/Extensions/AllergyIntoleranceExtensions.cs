using Hl7.Fhir.Model;
using MudBlazor;
using MudColor = MudBlazor.Color;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR AllergyIntolerance resources to simplify UI binding.
/// </summary>
public static class AllergyIntoleranceExtensions
{
    /// <summary>
    /// Gets the allergen name.
    /// </summary>
    public static string GetAllergen(this AllergyIntolerance allergy)
    {
        return allergy.Code?.Text ?? allergy.Code?.Coding?.FirstOrDefault()?.Display ?? "Unknown Allergen";
    }

    /// <summary>
    /// Gets the severity of the allergy.
    /// </summary>
    public static string GetSeverity(this AllergyIntolerance allergy)
    {
        return allergy.Criticality?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Gets the reaction manifestations.
    /// </summary>
    public static List<string> GetReactions(this AllergyIntolerance allergy)
    {
        return allergy.Reaction?
            .SelectMany(r => r.Manifestation)
            .Select(m => m.Concept?.Text ?? m.Concept?.Coding?.FirstOrDefault()?.Display ?? "Unknown")
            .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Gets the onset date of the allergy.
    /// </summary>
    public static DateTime? GetOnsetDate(this AllergyIntolerance allergy)
    {
        if (allergy.Onset is FhirDateTime dt)
            return dt.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        
        return null;
    }

    /// <summary>
    /// Gets the recorded date.
    /// </summary>
    public static DateTime GetRecordedDate(this AllergyIntolerance allergy)
    {
        if (!string.IsNullOrEmpty(allergy.RecordedDate))
            return DateTimeOffset.Parse(allergy.RecordedDate).DateTime;
        return DateTime.Now;
    }

    /// <summary>
    /// Gets the category (food, medication, environment, etc.).
    /// </summary>
    public static string GetCategory(this AllergyIntolerance allergy)
    {
        return allergy.Category?.FirstOrDefault()?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Gets the type (allergy or intolerance).
    /// </summary>
    public static string GetAllergyType(this AllergyIntolerance allergy)
    {
        return allergy.Type?.ToString() ?? "Allergy";
    }

    /// <summary>
    /// Gets notes/comments about the allergy.
    /// </summary>
    public static string GetNotes(this AllergyIntolerance allergy)
    {
        return allergy.Note?.FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the clinical status.
    /// </summary>
    public static string GetClinicalStatus(this AllergyIntolerance allergy)
    {
        return allergy.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? "active";
    }

    /// <summary>
    /// Gets MudBlazor color based on severity.
    /// </summary>
    public static MudColor GetSeverityColor(this AllergyIntolerance allergy)
    {
        return allergy.Criticality switch
        {
            AllergyIntolerance.AllergyIntoleranceCriticality.High => MudColor.Error,
            AllergyIntolerance.AllergyIntoleranceCriticality.Low => MudColor.Success,
            _ => MudColor.Warning
        };
    }

    /// <summary>
    /// Creates a new AllergyIntolerance resource.
    /// </summary>
    public static AllergyIntolerance CreateNew(
        string allergen,
        string category,
        string? severity = null,
        List<string>? reactions = null,
        DateTime? onsetDate = null,
        string? notes = null,
        string? patientId = null)
    {
        var allergy = new AllergyIntolerance
        {
            Id = Guid.NewGuid().ToString(),
            ClinicalStatus = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding("http://terminology.hl7.org/CodeSystem/allergyintolerance-clinical", "active")
                }
            },
            VerificationStatus = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding("http://terminology.hl7.org/CodeSystem/allergyintolerance-verification", "confirmed")
                }
            },
            Code = new CodeableConcept { Text = allergen },
            RecordedDate = DateTimeOffset.Now.ToString("o"),
            Category = new List<AllergyIntolerance.AllergyIntoleranceCategory?>
            {
                Enum.TryParse<AllergyIntolerance.AllergyIntoleranceCategory>(category, true, out var cat) 
                    ? cat 
                    : (AllergyIntolerance.AllergyIntoleranceCategory?)null
            }
        };

        if (!string.IsNullOrEmpty(patientId))
        {
            allergy.Patient = new ResourceReference($"Patient/{patientId}");
        }

        if (!string.IsNullOrEmpty(severity) && 
            Enum.TryParse<AllergyIntolerance.AllergyIntoleranceCriticality>(severity, true, out var crit))
        {
            allergy.Criticality = crit;
        }

        if (reactions?.Any() == true)
        {
            allergy.Reaction = reactions.Select(r => new AllergyIntolerance.ReactionComponent
            {
                Manifestation = new List<CodeableReference>
                {
                    new CodeableReference { Concept = new CodeableConcept { Text = r } }
                }
            }).ToList();
        }

        if (onsetDate.HasValue)
        {
            allergy.Onset = new FhirDateTime(onsetDate.Value);
        }

        if (!string.IsNullOrEmpty(notes))
        {
            allergy.Note = new List<Annotation>
            {
                new Annotation { Text = notes }
            };
        }

        return allergy;
    }

    /// <summary>
    /// Sets the allergen name.
    /// </summary>
    public static void SetAllergen(this AllergyIntolerance allergy, string allergen)
    {
        allergy.Code = new CodeableConcept { Text = allergen };
    }

    /// <summary>
    /// Sets the severity.
    /// </summary>
    public static void SetSeverity(this AllergyIntolerance allergy, string severity)
    {
        if (Enum.TryParse<AllergyIntolerance.AllergyIntoleranceCriticality>(severity, true, out var crit))
        {
            allergy.Criticality = crit;
        }
    }

    /// <summary>
    /// Sets the reactions.
    /// </summary>
    public static void SetReactions(this AllergyIntolerance allergy, List<string> reactions)
    {
        allergy.Reaction = reactions.Select(r => new AllergyIntolerance.ReactionComponent
        {
            Manifestation = new List<CodeableReference>
            {
                new CodeableReference { Concept = new CodeableConcept { Text = r } }
            }
        }).ToList();
    }

    /// <summary>
    /// Sets notes.
    /// </summary>
    public static void SetNotes(this AllergyIntolerance allergy, string notes)
    {
        allergy.Note = new List<Annotation>
        {
            new Annotation { Text = notes }
        };
    }
}
