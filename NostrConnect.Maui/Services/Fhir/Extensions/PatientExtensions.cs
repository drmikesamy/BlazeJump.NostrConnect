using Hl7.Fhir.Model;
using MudBlazor;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Patient resources.
/// </summary>
public static class PatientExtensions
{
    /// <summary>
    /// Gets the patient's full name.
    /// </summary>
    public static string GetName(this Patient patient)
    {
        var humanName = patient.Name?.FirstOrDefault();
        if (humanName == null)
            return "Unknown";

        var firstName = humanName.Given?.FirstOrDefault() ?? string.Empty;
        var lastName = humanName.Family ?? string.Empty;
        
        return $"{firstName} {lastName}".Trim();
    }

    /// <summary>
    /// Gets the patient's first name.
    /// </summary>
    public static string? GetFirstName(this Patient patient)
    {
        return patient.Name?.FirstOrDefault()?.Given?.FirstOrDefault();
    }

    /// <summary>
    /// Gets the patient's last name.
    /// </summary>
    public static string? GetLastName(this Patient patient)
    {
        return patient.Name?.FirstOrDefault()?.Family;
    }

    /// <summary>
    /// Gets the patient's date of birth.
    /// </summary>
    public static DateTime? GetBirthDate(this Patient patient)
    {
        if (string.IsNullOrEmpty(patient.BirthDate))
            return null;

        if (DateTime.TryParse(patient.BirthDate, out var date))
            return date;

        return null;
    }

    /// <summary>
    /// Gets the patient's weight from extension data.
    /// </summary>
    public static decimal? GetWeight(this Patient patient)
    {
        var weightExt = patient.Extension?.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-weight");
        if (weightExt?.Value is Quantity quantity)
            return quantity.Value;

        return null;
    }

    /// <summary>
    /// Gets the patient's height from extension data.
    /// </summary>
    public static decimal? GetHeight(this Patient patient)
    {
        var heightExt = patient.Extension?.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-height");
        if (heightExt?.Value is Quantity quantity)
            return quantity.Value;

        return null;
    }

    /// <summary>
    /// Gets the patient's blood type from extension data.
    /// </summary>
    public static string? GetBloodType(this Patient patient)
    {
        var bloodTypeExt = patient.Extension?.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-blood-type");
        if (bloodTypeExt?.Value is FhirString fhirString)
            return fhirString.Value;

        return null;
    }

    /// <summary>
    /// Sets the patient's weight in extension data.
    /// </summary>
    public static void SetWeight(this Patient patient, decimal? weight)
    {
        patient.Extension ??= new List<Extension>();
        
        var weightExt = patient.Extension.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-weight");
        if (weightExt != null)
            patient.Extension.Remove(weightExt);

        if (weight.HasValue)
        {
            patient.Extension.Add(new Extension
            {
                Url = "http://desilo.health/fhir/StructureDefinition/patient-weight",
                Value = new Quantity
                {
                    Value = weight.Value,
                    Unit = "kg",
                    System = "http://unitsofmeasure.org",
                    Code = "kg"
                }
            });
        }
    }

    /// <summary>
    /// Sets the patient's height in extension data.
    /// </summary>
    public static void SetHeight(this Patient patient, decimal? height)
    {
        patient.Extension ??= new List<Extension>();
        
        var heightExt = patient.Extension.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-height");
        if (heightExt != null)
            patient.Extension.Remove(heightExt);

        if (height.HasValue)
        {
            patient.Extension.Add(new Extension
            {
                Url = "http://desilo.health/fhir/StructureDefinition/patient-height",
                Value = new Quantity
                {
                    Value = height.Value,
                    Unit = "cm",
                    System = "http://unitsofmeasure.org",
                    Code = "cm"
                }
            });
        }
    }

    /// <summary>
    /// Sets the patient's blood type in extension data.
    /// </summary>
    public static void SetBloodType(this Patient patient, string? bloodType)
    {
        patient.Extension ??= new List<Extension>();
        
        var bloodTypeExt = patient.Extension.FirstOrDefault(e => e.Url == "http://desilo.health/fhir/StructureDefinition/patient-blood-type");
        if (bloodTypeExt != null)
            patient.Extension.Remove(bloodTypeExt);

        if (!string.IsNullOrEmpty(bloodType))
        {
            patient.Extension.Add(new Extension
            {
                Url = "http://desilo.health/fhir/StructureDefinition/patient-blood-type",
                Value = new FhirString(bloodType)
            });
        }
    }

    /// <summary>
    /// Creates a new FHIR Patient resource.
    /// </summary>
    public static Patient CreatePatient(
        string firstName,
        string lastName,
        DateTime? birthDate,
        string publicKey,
        decimal? weight = null,
        decimal? height = null,
        string? bloodType = null)
    {
        var patient = new Patient
        {
            Id = publicKey,
            Active = true,
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Use = HumanName.NameUse.Official,
                    Family = lastName,
                    Given = new[] { firstName }
                }
            },
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow
            }
        };

        if (birthDate.HasValue)
        {
            patient.BirthDate = birthDate.Value.ToString("yyyy-MM-dd");
        }

        patient.SetWeight(weight);
        patient.SetHeight(height);
        patient.SetBloodType(bloodType);

        return patient;
    }

    /// <summary>
    /// Calculates BMI from height and weight.
    /// </summary>
    public static decimal? CalculateBMI(this Patient patient)
    {
        var height = patient.GetHeight();
        var weight = patient.GetWeight();

        if (!height.HasValue || !weight.HasValue)
            return null;

        var heightInMeters = height.Value / 100;
        return weight.Value / (heightInMeters * heightInMeters);
    }
}
