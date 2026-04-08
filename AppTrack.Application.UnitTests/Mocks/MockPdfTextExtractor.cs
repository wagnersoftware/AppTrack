using AppTrack.Application.Contracts.CvStorage;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockPdfTextExtractor
{
    public const string ExtractedText = "Extracted CV text";

    public static Mock<IPdfTextExtractor> GetMock()
    {
        var mock = new Mock<IPdfTextExtractor>();
        mock.Setup(e => e.ExtractText(It.IsAny<Stream>()))
            .Returns(ExtractedText);
        return mock;
    }
}
