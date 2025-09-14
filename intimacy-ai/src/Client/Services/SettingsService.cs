using Microsoft.JSInterop;

namespace IntimacyAI.Client.Services
{
    public sealed class SettingsService
    {
        private readonly IJSRuntime _js;

        public SettingsService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetApiBaseUrlAsync() => await _js.InvokeAsync<string?>("settingsStore.get", "ApiBaseUrl");
        public async Task SetApiBaseUrlAsync(string? value) => await _js.InvokeVoidAsync("settingsStore.set", "ApiBaseUrl", value ?? "");
        public async Task<string?> GetApiKeyAsync() => await _js.InvokeAsync<string?>("settingsStore.get", "ApiKey");
        public async Task SetApiKeyAsync(string? value) => await _js.InvokeVoidAsync("settingsStore.set", "ApiKey", value ?? "");
    }
}