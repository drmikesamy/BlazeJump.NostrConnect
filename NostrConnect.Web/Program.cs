using NostrConnect.Web.Components;
using NostrConnect.Shared.Services;
using NostrConnect.Web.Services;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Connections.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Nostr services
// Note: Using Scoped lifetime for services that depend on IJSRuntime (which is scoped in Blazor Server)
builder.Services.AddScoped<IBrowserCrypto, BrowserCrypto>();
builder.Services.AddScoped<IKeyStorageService, WebKeyStorageService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IRelayConnectionProvider, RelayConnectionProvider>();
builder.Services.AddSingleton<IRelayManager, RelayManager>();
builder.Services.AddScoped<INostrService, NostrService>();
builder.Services.AddScoped<INostrConnectService, NostrConnectService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
