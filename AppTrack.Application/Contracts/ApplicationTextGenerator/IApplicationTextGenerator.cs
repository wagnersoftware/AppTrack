namespace AppTrack.Application.Contracts.ApplicationTextGenerator;

public interface IApplicationTextGenerator
{
    Task<string> GenerateApplicationTextAsync(string prompt, CancellationToken cancelationToken = default);
    void SetApiKey(string apiKey);
}
