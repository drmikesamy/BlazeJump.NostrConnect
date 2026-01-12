using Hl7.Fhir.Model;

namespace NostrConnect.Maui.Services.Fhir.Extensions;

/// <summary>
/// Extension methods for FHIR Appointment resources to simplify UI binding.
/// </summary>
public static class AppointmentExtensions
{
    /// <summary>
    /// Gets the title/description of the appointment.
    /// </summary>
    public static string GetTitle(this Appointment appointment)
    {
        return appointment.Description ?? "Untitled Appointment";
    }

    /// <summary>
    /// Gets the provider/practitioner name from participants.
    /// </summary>
    public static string GetProvider(this Appointment appointment)
    {
        var practitioner = appointment.Participant?
            .FirstOrDefault(p => p.Actor?.Reference?.StartsWith("Practitioner/") == true);
        
        return practitioner?.Actor?.Display ?? "Unknown Provider";
    }

    /// <summary>
    /// Gets the appointment date and time.
    /// </summary>
    public static DateTime GetDateTime(this Appointment appointment)
    {
        return appointment.Start?.DateTime ?? DateTime.MinValue;
    }

    /// <summary>
    /// Gets the appointment end date and time.
    /// </summary>
    public static DateTime? GetEndDateTime(this Appointment appointment)
    {
        return appointment.End?.DateTime;
    }

    /// <summary>
    /// Gets the duration in minutes.
    /// </summary>
    public static int GetMinutesDuration(this Appointment appointment)
    {
        return appointment.MinutesDuration ?? 30;
    }

    /// <summary>
    /// Gets the appointment status as a string.
    /// </summary>
    public static string GetStatusString(this Appointment appointment)
    {
        return appointment.Status?.ToString()?.ToLower() ?? "proposed";
    }

    /// <summary>
    /// Gets the location name.
    /// </summary>
    public static string GetLocation(this Appointment appointment)
    {
        var location = appointment.Participant?
            .FirstOrDefault(p => p.Actor?.Reference?.StartsWith("Location/") == true);
        
        return location?.Actor?.Display ?? string.Empty;
    }

    /// <summary>
    /// Gets the notes/comments from the appointment.
    /// </summary>
    public static string GetNotes(this Appointment appointment)
    {
        return appointment.Note?.FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the service category.
    /// </summary>
    public static string GetServiceCategory(this Appointment appointment)
    {
        return appointment.ServiceCategory?.FirstOrDefault()?.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the service type.
    /// </summary>
    public static string GetServiceType(this Appointment appointment)
    {
        return appointment.ServiceType?.FirstOrDefault()?.Concept?.Text ?? string.Empty;
    }

    /// <summary>
    /// Checks if the appointment is in the future.
    /// </summary>
    public static bool IsUpcoming(this Appointment appointment)
    {
        return appointment.GetDateTime() >= DateTime.Now;
    }

    /// <summary>
    /// Checks if the appointment is in the past.
    /// </summary>
    public static bool IsPast(this Appointment appointment)
    {
        return appointment.GetDateTime() < DateTime.Now;
    }

    /// <summary>
    /// Sets the title/description of the appointment.
    /// </summary>
    public static void SetTitle(this Appointment appointment, string title)
    {
        appointment.Description = title;
    }

    /// <summary>
    /// Sets the provider/practitioner for the appointment.
    /// </summary>
    public static void SetProvider(this Appointment appointment, string providerName, string? patientId = null)
    {
        appointment.Participant ??= new List<Appointment.ParticipantComponent>();
        
        // Remove existing practitioner
        var existingPractitioner = appointment.Participant
            .FirstOrDefault(p => p.Actor?.Reference?.StartsWith("Practitioner/") == true);
        if (existingPractitioner != null)
            appointment.Participant.Remove(existingPractitioner);

        // Add new practitioner
        appointment.Participant.Add(new Appointment.ParticipantComponent
        {
            Actor = new ResourceReference
            {
                Reference = $"Practitioner/{Guid.NewGuid()}",
                Display = providerName
            },
            Status = Appointment.ParticipationStatus.Accepted,
            Required = true
        });

        // Ensure patient is in participants
        if (!string.IsNullOrEmpty(patientId))
        {
            var existingPatient = appointment.Participant
                .FirstOrDefault(p => p.Actor?.Reference?.StartsWith("Patient/") == true);
            
            if (existingPatient == null)
            {
                appointment.Participant.Add(new Appointment.ParticipantComponent
                {
                    Actor = new ResourceReference
                    {
                        Reference = $"Patient/{patientId}",
                        Display = "Patient"
                    },
                    Status = Appointment.ParticipationStatus.Accepted,
                    Required = true
                });
            }
        }
    }

    /// <summary>
    /// Sets the appointment date and time.
    /// </summary>
    public static void SetDateTime(this Appointment appointment, DateTime dateTime, int durationMinutes = 30)
    {
        appointment.Start = dateTime;
        appointment.End = dateTime.AddMinutes(durationMinutes);
        appointment.MinutesDuration = durationMinutes;
    }

    /// <summary>
    /// Sets the location for the appointment.
    /// </summary>
    public static void SetLocation(this Appointment appointment, string locationName)
    {
        appointment.Participant ??= new List<Appointment.ParticipantComponent>();
        
        // Remove existing location
        var existingLocation = appointment.Participant
            .FirstOrDefault(p => p.Actor?.Reference?.StartsWith("Location/") == true);
        if (existingLocation != null)
            appointment.Participant.Remove(existingLocation);

        if (!string.IsNullOrWhiteSpace(locationName))
        {
            appointment.Participant.Add(new Appointment.ParticipantComponent
            {
                Actor = new ResourceReference
                {
                    Reference = $"Location/{Guid.NewGuid()}",
                    Display = locationName
                },
                Status = Appointment.ParticipationStatus.Accepted
            });
        }
    }

    /// <summary>
    /// Sets the notes/comments for the appointment.
    /// </summary>
    public static void SetNotes(this Appointment appointment, string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            appointment.Note = null;
            return;
        }

        appointment.Note = new List<Annotation>
        {
            new Annotation { Text = notes }
        };
    }

    /// <summary>
    /// Sets the service category.
    /// </summary>
    public static void SetServiceCategory(this Appointment appointment, string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            appointment.ServiceCategory = null;
            return;
        }

        appointment.ServiceCategory = new List<CodeableConcept>
        {
            new CodeableConcept { Text = category }
        };
    }

    /// <summary>
    /// Sets the service type.
    /// </summary>
    public static void SetServiceType(this Appointment appointment, string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
        {
            appointment.ServiceType = null;
            return;
        }

        appointment.ServiceType = new List<CodeableReference>
        {
            new CodeableReference
            {
                Concept = new CodeableConcept { Text = serviceType }
            }
        };
    }

    /// <summary>
    /// Sets the appointment status from a string.
    /// </summary>
    public static void SetStatus(this Appointment appointment, string status)
    {
        appointment.Status = status?.ToLower() switch
        {
            "proposed" => Appointment.AppointmentStatus.Proposed,
            "pending" => Appointment.AppointmentStatus.Pending,
            "booked" => Appointment.AppointmentStatus.Booked,
            "arrived" => Appointment.AppointmentStatus.Arrived,
            "fulfilled" => Appointment.AppointmentStatus.Fulfilled,
            "cancelled" => Appointment.AppointmentStatus.Cancelled,
            "noshow" => Appointment.AppointmentStatus.Noshow,
            _ => Appointment.AppointmentStatus.Proposed
        };
    }

    /// <summary>
    /// Creates a new FHIR Appointment with basic information.
    /// </summary>
    public static Appointment CreateNew(
        string title,
        string provider,
        DateTime dateTime,
        int durationMinutes = 30,
        string? patientId = null)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid().ToString(),
            Status = Appointment.AppointmentStatus.Proposed,
            Description = title,
            Start = dateTime,
            End = dateTime.AddMinutes(durationMinutes),
            MinutesDuration = durationMinutes,
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow
            }
        };

        appointment.SetProvider(provider, patientId);
        
        return appointment;
    }
}
