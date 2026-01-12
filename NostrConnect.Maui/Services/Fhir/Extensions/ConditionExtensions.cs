using Hl7.Fhir.Model;
using MudBlazor;
using FhirCondition = Hl7.Fhir.Model.Condition;
using MudColor = MudBlazor.Color;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Condition resources (Medical History) to simplify UI binding.
/// </summary>
public static class ConditionExtensions
{
    /// <summary>
    /// Gets the condition name/diagnosis.
    /// </summary>
    public static string GetConditionName(this FhirCondition condition)
    {
        return condition.Code?.Text ?? 
               condition.Code?.Coding?.FirstOrDefault()?.Display ?? 
               "Unknown Condition";
    }

    /// <summary>
    /// Gets the clinical status (active, inactive, resolved, etc.).
    /// </summary>
    public static string GetClinicalStatus(this FhirCondition condition)
    {
        return condition.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? "unknown";
    }

    /// <summary>
    /// Gets the verification status (confirmed, provisional, etc.).
    /// </summary>
    public static string GetVerificationStatus(this FhirCondition condition)
    {
        return condition.VerificationStatus?.Coding?.FirstOrDefault()?.Code ?? "unknown";
    }

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public static string GetSeverity(this FhirCondition condition)
    {
        return condition.Severity?.Text ?? 
               condition.Severity?.Coding?.FirstOrDefault()?.Display ?? 
               "Unknown";
    }

    /// <summary>
    /// Gets the onset date.
    /// </summary>
    public static DateTime? GetOnsetDate(this FhirCondition condition)
    {
        return condition.Onset switch
        {
            FhirDateTime dt => dt.ToDateTimeOffset(TimeSpan.Zero).DateTime,
            Age age => DateTime.Now.AddYears(-(int)(age.Value ?? 0)),
            Period period when period.Start != null => 
                DateTimeOffset.Parse(period.Start).DateTime,
            _ => null
        };
    }

    /// <summary>
    /// Gets the recorded date.
    /// </summary>
    public static DateTime GetRecordedDate(this FhirCondition condition)
    {
        if (!string.IsNullOrEmpty(condition.RecordedDate))
            return DateTimeOffset.Parse(condition.RecordedDate).DateTime;
        return DateTime.Now;
    }

    /// <summary>
    /// Gets the abatement (resolution) date.
    /// </summary>
    public static DateTime? GetAbatementDate(this FhirCondition condition)
    {
        return condition.Abatement switch
        {
            FhirDateTime dt => dt.ToDateTimeOffset(TimeSpan.Zero).DateTime,
            Period period when period.End != null => 
                DateTimeOffset.Parse(period.End).DateTime,
            _ => null
        };
    }

    /// <summary>
    /// Gets notes about the condition.
    /// </summary>
    public static string GetNotes(this FhirCondition condition)
    {
        return condition.Note?.FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the body site affected.
    /// </summary>
    public static string GetBodySite(this FhirCondition condition)
    {
        return condition.BodySite?.FirstOrDefault()?.Text ?? 
               condition.BodySite?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Display ?? 
               string.Empty;
    }

    /// <summary>
    /// Gets the category (problem-list-item, encounter-diagnosis, etc.).
    /// </summary>
    public static string GetCategory(this FhirCondition condition)
    {
        return condition.Category?.FirstOrDefault()?.Coding?.FirstOrDefault()?.Code ?? "unknown";
    }

    /// <summary>
    /// Checks if the condition is currently active.
    /// </summary>
    public static bool IsActive(this FhirCondition condition)
    {
        var status = condition.GetClinicalStatus().ToLower();
        return status == "active" || status == "recurrence" || status == "relapse";
    }

    /// <summary>
    /// Gets MudBlazor color based on clinical status.
    /// </summary>
    public static MudColor GetStatusColor(this FhirCondition condition)
    {
        return condition.GetClinicalStatus().ToLower() switch
        {
            "active" => MudColor.Error,
            "recurrence" => MudColor.Warning,
            "relapse" => MudColor.Warning,
            "inactive" => MudColor.Info,
            "remission" => MudColor.Success,
            "resolved" => MudColor.Success,
            _ => MudColor.Default
        };
    }

    /// <summary>
    /// Gets an icon based on the condition status.
    /// </summary>
    public static string GetStatusIcon(this FhirCondition condition)
    {
        return condition.GetClinicalStatus().ToLower() switch
        {
            "active" => Icons.Material.Filled.LocalHospital,
            "resolved" => Icons.Material.Filled.CheckCircle,
            "inactive" => Icons.Material.Filled.Pause,
            _ => Icons.Material.Filled.MedicalInformation
        };
    }

    /// <summary>
    /// Creates a new Condition resource.
    /// </summary>
    public static FhirCondition CreateNew(
        string conditionName,
        DateTime? onsetDate = null,
        string? severity = null,
        string? bodySite = null,
        string? notes = null,
        string? patientId = null)
    {
        var condition = new FhirCondition
        {
            Id = Guid.NewGuid().ToString(),
            ClinicalStatus = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding("http://terminology.hl7.org/CodeSystem/condition-clinical", "active")
                }
            },
            VerificationStatus = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding("http://terminology.hl7.org/CodeSystem/condition-ver-status", "confirmed")
                }
            },
            Code = new CodeableConcept { Text = conditionName },
            RecordedDate = DateTimeOffset.Now.ToString("o"),
            Category = new List<CodeableConcept>
            {
                new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/condition-category", "problem-list-item")
                    }
                }
            }
        };

        if (!string.IsNullOrEmpty(patientId))
        {
            condition.Subject = new ResourceReference($"Patient/{patientId}");
        }

        if (onsetDate.HasValue)
        {
            condition.Onset = new FhirDateTime(onsetDate.Value);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            condition.Severity = new CodeableConcept { Text = severity };
        }

        if (!string.IsNullOrEmpty(bodySite))
        {
            condition.BodySite = new List<CodeableConcept>
            {
                new CodeableConcept { Text = bodySite }
            };
        }

        if (!string.IsNullOrEmpty(notes))
        {
            condition.Note = new List<Annotation>
            {
                new Annotation { Text = notes }
            };
        }

        return condition;
    }

    /// <summary>
    /// Sets the condition name.
    /// </summary>
    public static void SetConditionName(this FhirCondition condition, string name)
    {
        condition.Code = new CodeableConcept { Text = name };
    }

    /// <summary>
    /// Sets the clinical status.
    /// </summary>
    public static void SetClinicalStatus(this FhirCondition condition, string status)
    {
        condition.ClinicalStatus = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new Coding("http://terminology.hl7.org/CodeSystem/condition-clinical", status.ToLower())
            }
        };
    }

    /// <summary>
    /// Sets the onset date.
    /// </summary>
    public static void SetOnsetDate(this FhirCondition condition, DateTime date)
    {
        condition.Onset = new FhirDateTime(date);
    }

    /// <summary>
    /// Sets the severity.
    /// </summary>
    public static void SetSeverity(this FhirCondition condition, string severity)
    {
        condition.Severity = new CodeableConcept { Text = severity };
    }

    /// <summary>
    /// Sets notes.
    /// </summary>
    public static void SetNotes(this FhirCondition condition, string notes)
    {
        condition.Note = new List<Annotation>
        {
            new Annotation { Text = notes }
        };
    }

    /// <summary>
    /// Marks the condition as resolved.
    /// </summary>
    public static void MarkAsResolved(this FhirCondition condition, DateTime? resolutionDate = null)
    {
        condition.SetClinicalStatus("resolved");
        condition.Abatement = new FhirDateTime(resolutionDate ?? DateTime.Now);
    }
}
