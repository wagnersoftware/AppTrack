using AppTrack.Domain.Enums;

namespace AppTrack.Application.Contracts.AiTextGenerator;

public interface IAiTextGenerator
{
    Task<string> GenerateAiTextAsync(string prompt, string modelName, AiResponseLanguage language, CancellationToken cancellationToken = default);
}
