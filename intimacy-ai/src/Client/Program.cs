using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IntimacyAI.Client;
using System.Net.Http.Json;
using IntimacyAI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<SettingsService>();

var tempHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var settingsFile = await tempHttp.GetFromJsonAsync<AppSettings>("appsettings.json");
var fallbackApiBase = settingsFile?.ApiBaseUrl ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(async sp =>
{
    var svc = sp.GetRequiredService<SettingsService>();
    var stored = await svc.GetApiBaseUrlAsync();
    var baseUrl = string.IsNullOrWhiteSpace(stored) ? fallbackApiBase : stored;
    var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    // Attach JWT token if present
    var token = await svc.GetValue("JwtToken");
    if (!string.IsNullOrWhiteSpace(token))
    {
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
    return http;
});

await builder.Build().RunAsync();

public sealed class AppSettings
{
    public string? ApiBaseUrl { get; set; }
}
