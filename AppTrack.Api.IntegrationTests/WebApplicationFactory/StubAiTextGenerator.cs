using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Domain.Enums;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

internal sealed class StubAiTextGenerator : IAiTextGenerator
{
    public const string FakeGeneratedText = "Integration test generated text.";

    public Task<string> GenerateAiTextAsync(
        string prompt,
        string modelName,
        AiResponseLanguage language,
        CancellationToken cancellationToken = default)
        => Task.FromResult(FakeGeneratedText);
}
