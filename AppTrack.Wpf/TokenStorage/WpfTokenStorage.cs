using AppTrack.Frontend.ApiService.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace AppTrack.WpfUi.TokenStorage
{
    // todo dependency injection
    public class WpfTokenStorage : ITokenStorage
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AppTrack", "token.json");

        public async Task<bool> ContainsKeyAsync(string key)
        {
            if (!File.Exists(_filePath)) return false;
            var json = await File.ReadAllTextAsync(_filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            return dict.ContainsKey(key);
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            if (!File.Exists(_filePath)) return default;
            var json = await File.ReadAllTextAsync(_filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            if (!dict.TryGetValue(key, out var value)) return default;

            var decrypted = ProtectedData.Unprotect(
                Convert.FromBase64String(value), null, DataProtectionScope.CurrentUser);

            return JsonSerializer.Deserialize<T>(decrypted);
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            Dictionary<string, string> dict = new();
            if (File.Exists(_filePath))
            {
                var existingJson = await File.ReadAllTextAsync(_filePath);
                dict = JsonSerializer.Deserialize<Dictionary<string, string>>(existingJson) ?? new();
            }

            var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
            var encrypted = ProtectedData.Protect(serialized, null, DataProtectionScope.CurrentUser);
            dict[key] = Convert.ToBase64String(encrypted);

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(dict));
        }

        public async Task RemoveItemAsync(string key)
        {
            if (!File.Exists(_filePath)) return;
            var json = await File.ReadAllTextAsync(_filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            dict.Remove(key);
            await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(dict));
        }
    }
}
