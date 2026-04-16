using AppTrack.Domain.Enums;

namespace AppTrack.Application.Contracts.ApplicationTextGenerator;

public interface IApplicationTextGenerator
{
    Task<string> GenerateApplicationTextAsync(string prompt, string modelName, AiResponseLanguage language, CancellationToken cancellationToken = default);
}
