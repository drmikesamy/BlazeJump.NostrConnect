using Microsoft.Extensions.Logging;
using NostrConnect.Maui.Services;
using NostrConnect.Maui.Services.Crypto;
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
        builder.Services.AddScoped<IHealthDataService, HealthDataService>();

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

        // Initialize database
        InitializeDatabase(app.Services).Wait();

        return app;
	}

    private static async Task InitializeDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<NostrDbContext>>();
        using var context = await contextFactory.CreateDbContextAsync();
        
        // Check if database needs migration by checking if HealthData table exists
        var needsMigration = false;
        try
        {
            await context.HealthData.AnyAsync();
        }
        catch
        {
            needsMigration = true;
        }

        if (needsMigration)
        {
            // Save existing profiles before recreating database
            List<UserProfile> existingProfiles = new List<UserProfile>();
            try
            {
                existingProfiles = await context.UserProfiles.ToListAsync();
            }
            catch
            {
                // If profiles table doesn't exist, that's ok
            }

            // Delete and recreate database to ensure schema is correct
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Restore profiles after database recreation
            if (existingProfiles.Any())
            {
                context.UserProfiles.AddRange(existingProfiles);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // No migration needed, just ensure database is created
            await context.Database.EnsureCreatedAsync();
        }

        // Check if we need to restore profile from SecureStorage
        var currentUserPubkey = Preferences.Default.Get("current_user_pubkey", string.Empty);
        if (!string.IsNullOrEmpty(currentUserPubkey))
        {
            var existingProfile = await context.UserProfiles
                .FirstOrDefaultAsync(p => p.PublicKey == currentUserPubkey);
            
            if (existingProfile == null)
            {
                // Profile exists in SecureStorage but not in database - restore it
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
