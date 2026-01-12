using Microsoft.Extensions.Logging;
using NostrConnect.Maui.Services;
using NostrConnect.Maui.Services.Crypto;
using NostrConnect.Maui.Services.Fhir;
using NostrConnect.Maui.Data;
using BlazeJump.Tools;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Connections.Providers;
using ZXing.Net.Maui.Controls;
using Microsoft.EntityFrameworkCore;
using NostrConnect.Maui.Services.Identity;
using BlazeJump.Tools.Services.Identity;
using BlazeJump.Tools.Services.Persistence;
using MudBlazor.Services;
using BlazeJump.Tools.Models;
using Hl7.Fhir.Model;

namespace NostrConnect.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        // Configure SQLite database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "nostr.db");
        builder.Services.AddDbContextFactory<NostrDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddSingleton<INostrDataService, NostrDataService>();
        
        // Add FHIR services - generic for all resource types
        builder.Services.AddScoped<IFhirResourceService<Appointment>, FhirResourceService<Appointment>>();
        builder.Services.AddScoped<IFhirResourceService<Observation>, FhirResourceService<Observation>>();
        builder.Services.AddScoped<IFhirResourceService<Medication>, FhirResourceService<Medication>>();
        builder.Services.AddScoped<IFhirResourceService<AllergyIntolerance>, FhirResourceService<AllergyIntolerance>>();
        builder.Services.AddScoped<IFhirResourceService<Immunization>, FhirResourceService<Immunization>>();
        builder.Services.AddScoped<IFhirResourceService<Hl7.Fhir.Model.Condition>, FhirResourceService<Hl7.Fhir.Model.Condition>>();

        CommonServices.ConfigureServices(builder.Services);
        builder.Services.AddSingleton<INativeIdentityService, NativeIdentityService>();
		builder.Services.AddScoped<ICryptoService, NativeCryptoService>();
		
		// Add MudBlazor services
		builder.Services.AddMudServices();
		
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
		
		// Enable detailed Blazor logs
		builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Debug);
		builder.Logging.AddFilter("Microsoft.AspNetCore.Components.RenderTree", LogLevel.Debug);
#endif

        var app = builder.Build();

        InitializeDatabase(app.Services).Wait();

        return app;
	}

    private static async System.Threading.Tasks.Task InitializeDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<NostrDbContext>>();
        using var context = await contextFactory.CreateDbContextAsync();
        
        var needsMigration = false;
        try
        {
            await context.LocalResources.AnyAsync();
        }
        catch
        {
            needsMigration = true;
        }

        if (needsMigration)
        {
            List<UserProfile> existingProfiles = new List<UserProfile>();
            try
            {
                existingProfiles = await context.UserProfiles.ToListAsync();
            }
            catch
            {
            }

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            if (existingProfiles.Any())
            {
                context.UserProfiles.AddRange(existingProfiles);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        var currentUserPubkey = Preferences.Default.Get("current_user_pubkey", string.Empty);
        if (!string.IsNullOrEmpty(currentUserPubkey))
        {
            var existingProfile = await context.UserProfiles
                .FirstOrDefaultAsync(p => p.PublicKey == currentUserPubkey);
            
            if (existingProfile == null)
            {
                var privateKey = await SecureStorage.Default.GetAsync($"blazejumpuserkeypair_{currentUserPubkey}");
                if (!string.IsNullOrEmpty(privateKey))
                {
                    var restoredProfile = new UserProfile
                    {
                        PublicKey = currentUserPubkey,
                        IsCurrentUser = true,
                        LastUpdated = DateTime.UtcNow
                    };
                    
                    context.UserProfiles.Add(restoredProfile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
