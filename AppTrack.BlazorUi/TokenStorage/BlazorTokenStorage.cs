using AppTrack.Frontend.ApiService.Contracts;
using Microsoft.JSInterop;
using System.Text.Json;

namespace AppTrack.BlazorUi.TokenStorage;

public class BlazorTokenStorage(IJSRuntime jsRuntime) : ITokenStorage
{
    public async Task<bool> ContainsKeyAsync(string key)
    {
        var value = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        return value is not null;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (json is null) return default;
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
