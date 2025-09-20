namespace AppTrack.Frontend.ApiService.Contracts;

public interface ITokenStorage
{
    Task<bool> ContainsKeyAsync(string key);
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}
