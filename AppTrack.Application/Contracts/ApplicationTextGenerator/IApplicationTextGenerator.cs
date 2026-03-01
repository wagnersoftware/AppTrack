namespace AppTrack.Application.Contracts.ApplicationTextGenerator;

public interface IApplicationTextGenerator
{
    Task<string> GenerateApplicationTextAsync(string prompt, string modelName, CancellationToken cancellationToken = default);
    void SetApiKey(string apiKey);
}
