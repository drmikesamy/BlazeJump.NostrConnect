using NostrConnect.Web.Components;
using BlazeJump.Tools;
using BlazeJump.Tools.Services.Crypto;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using NostrConnect.Web.Services.Crypto;
using NostrConnect.Web.Services.Identity;
using BlazeJump.Tools.Services.Identity;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

CommonServices.ConfigureServices(builder.Services);

builder.Services.AddScoped<ICryptoService, WebCryptoService>();
builder.Services.AddScoped<IWebIdentityService, WebIdentityService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
