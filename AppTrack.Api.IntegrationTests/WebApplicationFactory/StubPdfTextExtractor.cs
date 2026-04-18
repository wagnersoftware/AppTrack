using AppTrack.Application.Contracts.CvStorage;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

internal sealed class StubPdfTextExtractor : IPdfTextExtractor
{
    public const string FakeExtractedText = "Extracted CV text for integration test.";

    public string ExtractText(Stream stream) => FakeExtractedText;
}
