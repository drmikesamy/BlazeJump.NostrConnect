using Microsoft.Extensions.Logging;
using NostrConnect.Shared.Services;
using NostrConnect.Maui.Services;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Connections.Providers;
using ZXing.Net.Maui.Controls;

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

		// Register services
		builder.Services.AddSingleton<IKeyStorageService, MauiKeyStorageService>();
		builder.Services.AddSingleton<ICryptoService, MauiCryptoService>();
		builder.Services.AddSingleton<IRelayConnectionProvider, RelayConnectionProvider>();
		builder.Services.AddSingleton<IRelayManager, RelayManager>();
		builder.Services.AddSingleton<INostrService, NostrService>();
		builder.Services.AddSingleton<INostrConnectService, NostrConnectService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
