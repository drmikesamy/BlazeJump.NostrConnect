using NostrConnect.Maui.Models;
using NostrConnect.Maui.Data;
using NostrConnect.Maui.Services.Identity;
using Microsoft.EntityFrameworkCore;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Enums;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FhirAppointment = Hl7.Fhir.Model.Appointment;
using FhirMedication = Hl7.Fhir.Model.Medication;
using FhirResource = Hl7.Fhir.Model.Resource;

namespace NostrConnect.Maui.Services;

public interface IHealthDataService
{
    System.Threading.Tasks.Task<List<VitalSign>> GetVitalSignsAsync(string publicKey);
    System.Threading.Tasks.Task SaveVitalSignAsync(VitalSign vitalSign);
    System.Threading.Tasks.Task<List<Medication>> GetMedicationsAsync(string publicKey);
    System.Threading.Tasks.Task SaveMedicationAsync(Medication medication);
    System.Threading.Tasks.Task<List<Appointment>> GetAppointmentsAsync(string publicKey);
    System.Threading.Tasks.Task SaveAppointmentAsync(Appointment appointment);
}

public class HealthDataService : IHealthDataService
{
    private readonly IDbContextFactory<NostrDbContext> _contextFactory;
    private readonly IMessageService _messageService;
    private readonly ICryptoService _cryptoService;
    private readonly INativeIdentityService _identityService;

    public HealthDataService(
        IDbContextFactory<NostrDbContext> contextFactory,
        INativeIdentityService identityService,
        ICryptoService cryptoService,
        IMessageService messageService)
    {
        _contextFactory = contextFactory;
        _identityService = identityService;
        _cryptoService = cryptoService;
        _messageService = messageService;
    }

    public async System.Threading.Tasks.Task<List<VitalSign>> GetVitalSignsAsync(string publicKey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var records = await context.HealthData
            .Where(h => h.PublicKey == publicKey && h.Type == "VitalSign")
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();

        return records
            .Select(r => JsonConvert.DeserializeObject<VitalSign>(r.Data ?? "{}"))
            .Where(v => v != null)
            .Cast<VitalSign>()
            .ToList();
    }

    public async System.Threading.Tasks.Task SaveVitalSignAsync(VitalSign vitalSign)
    {
        var publicKey = _identityService.ActiveUserProfile?.PublicKey;
        if (string.IsNullOrEmpty(publicKey))
            return;

        vitalSign.Timestamp = DateTime.UtcNow;

        var dataJson = JsonConvert.SerializeObject(vitalSign);
        var encryptedData = await _cryptoService.Nip44Encrypt(dataJson, publicKey, publicKey);

        var nEvent = _messageService.CreateNEvent(
            publicKey,
            KindEnum.EncryptedDirectMessages,
            encryptedData,
            null,
            null,
            new List<string> { publicKey }
        );

        await _messageService.Send(KindEnum.EncryptedDirectMessages, nEvent, null);

        using var context = await _contextFactory.CreateDbContextAsync();
        var healthData = new HealthData
        {
            PublicKey = publicKey,
            Type = "VitalSign",
            Data = JsonConvert.SerializeObject(vitalSign),
            Timestamp = vitalSign.Timestamp,
            NostrEventId = nEvent.Id
        };
        context.HealthData.Add(healthData);
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<List<Medication>> GetMedicationsAsync(string publicKey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var records = await context.HealthData
            .Where(h => h.PublicKey == publicKey && h.Type == "Medication")
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();

        return records
            .Select(r => JsonConvert.DeserializeObject<Medication>(r.Data ?? "{}"))
            .Where(m => m != null)
            .Cast<Medication>()
            .ToList();
    }

    public async System.Threading.Tasks.Task SaveMedicationAsync(Medication medication)
    {
        var publicKey = _identityService.ActiveUserProfile?.PublicKey;
        if (string.IsNullOrEmpty(publicKey))
            return;

        var dataJson = JsonConvert.SerializeObject(medication);
        var encryptedData = await _cryptoService.Nip44Encrypt(dataJson, publicKey, publicKey);

        var nEvent = _messageService.CreateNEvent(
            publicKey,
            KindEnum.EncryptedDirectMessages,
            encryptedData,
            null,
            null,
            new List<string> { publicKey }
        );

        await _messageService.Send(KindEnum.EncryptedDirectMessages, nEvent, null);

        using var context = await _contextFactory.CreateDbContextAsync();
        var healthData = new HealthData
        {
            PublicKey = publicKey,
            Type = "Medication",
            Data = JsonConvert.SerializeObject(medication),
            Timestamp = DateTime.UtcNow,
            NostrEventId = nEvent.Id
        };
        context.HealthData.Add(healthData);
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<List<Appointment>> GetAppointmentsAsync(string publicKey)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var records = await context.HealthData
            .Where(h => h.PublicKey == publicKey && h.Type == "Appointment")
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();

        return records
            .Select(r => JsonConvert.DeserializeObject<Appointment>(r.Data ?? "{}"))
            .Where(a => a != null)
            .Cast<Appointment>()
            .ToList();
    }

    public async System.Threading.Tasks.Task SaveAppointmentAsync(Appointment appointment)
    {
        var publicKey = _identityService.ActiveUserProfile?.PublicKey;
        if (string.IsNullOrEmpty(publicKey))
            return;

        var dataJson = JsonConvert.SerializeObject(appointment);
        var encryptedData = await _cryptoService.Nip44Encrypt(dataJson, publicKey, publicKey);

        var nEvent = _messageService.CreateNEvent(
            publicKey,
            KindEnum.EncryptedDirectMessages,
            encryptedData,
            null,
            null,
            new List<string> { publicKey }
        );

        await _messageService.Send(KindEnum.EncryptedDirectMessages, nEvent, null);

        using var context = await _contextFactory.CreateDbContextAsync();
        var healthData = new HealthData
        {
            PublicKey = publicKey,
            Type = "Appointment",
            Data = JsonConvert.SerializeObject(appointment),
            Timestamp = DateTime.UtcNow,
            NostrEventId = nEvent.Id
        };
        context.HealthData.Add(healthData);
        await context.SaveChangesAsync();
    }
}
