using Hl7.Fhir.Model;
using MudBlazor;
using MudColor = MudBlazor.Color;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Immunization resources to simplify UI binding.
/// </summary>
public static class ImmunizationExtensions
{
    /// <summary>
    /// Gets the vaccine name.
    /// </summary>
    public static string GetVaccineName(this Immunization immunization)
    {
        return immunization.VaccineCode?.Text ?? 
               immunization.VaccineCode?.Coding?.FirstOrDefault()?.Display ?? 
               "Unknown Vaccine";
    }

    /// <summary>
    /// Gets the vaccination date.
    /// </summary>
    public static DateTime GetDate(this Immunization immunization)
    {
        if (immunization.Occurrence is FhirDateTime dt)
            return dt.ToDateTimeOffset(TimeSpan.Zero).DateTime;
        
        return DateTime.Now;
    }

    /// <summary>
    /// Gets the dose number (e.g., "Dose 1 of 2").
    /// </summary>
    public static string GetDoseNumber(this Immunization immunization)
    {
        if (immunization.ProtocolApplied?.Any() == true)
        {
            var protocol = immunization.ProtocolApplied.First();
            var doseNumber = protocol.DoseNumber?.ToString() ?? "Unknown";
            var seriesDoses = protocol.SeriesDoses?.ToString();
            
            if (!string.IsNullOrEmpty(seriesDoses))
                return $"Dose {doseNumber} of {seriesDoses}";
            
            return $"Dose {doseNumber}";
        }
        
        return "Single Dose";
    }

    /// <summary>
    /// Gets the manufacturer.
    /// </summary>
    public static string GetManufacturer(this Immunization immunization)
    {
        return immunization.Manufacturer?.Reference?.Display ?? "Unknown Manufacturer";
    }

    /// <summary>
    /// Gets the lot number.
    /// </summary>
    public static string GetLotNumber(this Immunization immunization)
    {
        return immunization.LotNumber ?? "N/A";
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    public static string GetStatus(this Immunization immunization)
    {
        return immunization.Status?.ToString() ?? "Completed";
    }

    /// <summary>
    /// Gets the site of administration.
    /// </summary>
    public static string GetSite(this Immunization immunization)
    {
        return immunization.Site?.Text ?? 
               immunization.Site?.Coding?.FirstOrDefault()?.Display ?? 
               "N/A";
    }

    /// <summary>
    /// Gets the route of administration.
    /// </summary>
    public static string GetRoute(this Immunization immunization)
    {
        return immunization.Route?.Text ?? 
               immunization.Route?.Coding?.FirstOrDefault()?.Display ?? 
               "N/A";
    }

    /// <summary>
    /// Gets notes about the immunization.
    /// </summary>
    public static string GetNotes(this Immunization immunization)
    {
        return immunization.Note?.FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the performer (who administered).
    /// </summary>
    public static string GetPerformer(this Immunization immunization)
    {
        return immunization.Performer?.FirstOrDefault()?.Actor?.Display ?? "Unknown";
    }

    /// <summary>
    /// Gets MudBlazor color based on status.
    /// </summary>
    public static MudColor GetStatusColor(this Immunization immunization)
    {
        return immunization.Status switch
        {
            Immunization.ImmunizationStatusCodes.Completed => MudColor.Success,
            Immunization.ImmunizationStatusCodes.NotDone => MudColor.Error,
            Immunization.ImmunizationStatusCodes.EnteredInError => MudColor.Warning,
            _ => MudColor.Default
        };
    }

    /// <summary>
    /// Gets an icon for the immunization status.
    /// </summary>
    public static string GetStatusIcon(this Immunization immunization)
    {
        return immunization.Status switch
        {
            Immunization.ImmunizationStatusCodes.Completed => Icons.Material.Filled.CheckCircle,
            Immunization.ImmunizationStatusCodes.NotDone => Icons.Material.Filled.Cancel,
            _ => Icons.Material.Filled.Vaccines
        };
    }

    /// <summary>
    /// Creates a new Immunization resource.
    /// </summary>
    public static Immunization CreateNew(
        string vaccineName,
        DateTime date,
        int? doseNumber = null,
        int? seriesDoses = null,
        string? manufacturer = null,
        string? lotNumber = null,
        string? site = null,
        string? route = null,
        string? performer = null,
        string? notes = null,
        string? patientId = null)
    {
        var immunization = new Immunization
        {
            Id = Guid.NewGuid().ToString(),
            Status = Immunization.ImmunizationStatusCodes.Completed,
            VaccineCode = new CodeableConcept { Text = vaccineName },
            Occurrence = new FhirDateTime(date)
        };

        if (!string.IsNullOrEmpty(patientId))
        {
            immunization.Patient = new ResourceReference($"Patient/{patientId}");
        }

        if (doseNumber.HasValue)
        {
            immunization.ProtocolApplied = new List<Immunization.ProtocolAppliedComponent>
            {
                new Immunization.ProtocolAppliedComponent
                {
                    DoseNumber = doseNumber.Value.ToString(),
                    SeriesDoses = seriesDoses?.ToString()
                }
            };
        }

        if (!string.IsNullOrEmpty(manufacturer))
        {
            immunization.Manufacturer = new CodeableReference 
            { 
                Reference = new ResourceReference { Display = manufacturer }
            };
        }

        if (!string.IsNullOrEmpty(lotNumber))
        {
            immunization.LotNumber = lotNumber;
        }

        if (!string.IsNullOrEmpty(site))
        {
            immunization.Site = new CodeableConcept { Text = site };
        }

        if (!string.IsNullOrEmpty(route))
        {
            immunization.Route = new CodeableConcept { Text = route };
        }

        if (!string.IsNullOrEmpty(performer))
        {
            immunization.Performer = new List<Immunization.PerformerComponent>
            {
                new Immunization.PerformerComponent
                {
                    Actor = new ResourceReference { Display = performer }
                }
            };
        }

        if (!string.IsNullOrEmpty(notes))
        {
            immunization.Note = new List<Annotation>
            {
                new Annotation { Text = notes }
            };
        }

        return immunization;
    }

    /// <summary>
    /// Sets the vaccine name.
    /// </summary>
    public static void SetVaccineName(this Immunization immunization, string name)
    {
        immunization.VaccineCode = new CodeableConcept { Text = name };
    }

    /// <summary>
    /// Sets the date.
    /// </summary>
    public static void SetDate(this Immunization immunization, DateTime date)
    {
        immunization.Occurrence = new FhirDateTime(date);
    }

    /// <summary>
    /// Sets the manufacturer.
    /// </summary>
    public static void SetManufacturer(this Immunization immunization, string manufacturer)
    {
        immunization.Manufacturer = new CodeableReference 
        { 
            Reference = new ResourceReference { Display = manufacturer }
        };
    }

    /// <summary>
    /// Sets notes.
    /// </summary>
    public static void SetNotes(this Immunization immunization, string notes)
    {
        immunization.Note = new List<Annotation>
        {
            new Annotation { Text = notes }
        };
    }
}
