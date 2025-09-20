namespace AppTrack.BlazorUI.TokenStorage;

using AppTrack.Frontend.ApiService.Contracts;
using Blazored.LocalStorage;

public class BlazorTokenStorage : ITokenStorage
{
    private readonly ILocalStorageService _localStorage;

    public BlazorTokenStorage(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public Task<bool> ContainsKeyAsync(string key) =>
        _localStorage.ContainKeyAsync(key).AsTask();

    public Task<T?> GetItemAsync<T>(string key) =>
        _localStorage.GetItemAsync<T>(key).AsTask();

    public Task SetItemAsync<T>(string key, T value) =>
        _localStorage.SetItemAsync(key, value).AsTask();

    public Task RemoveItemAsync(string key) =>
        _localStorage.RemoveItemAsync(key).AsTask();
}
