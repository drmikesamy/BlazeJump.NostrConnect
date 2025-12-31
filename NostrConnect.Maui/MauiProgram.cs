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

        // Register data service
        builder.Services.AddSingleton<INostrDataService, NostrDataService>();

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
        await context.Database.EnsureCreatedAsync();
    }
}
