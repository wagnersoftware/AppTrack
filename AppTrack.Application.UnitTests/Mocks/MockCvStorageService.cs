using AppTrack.Application.Contracts.CvStorage;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockCvStorageService
{
    public static Mock<ICvStorageService> GetMock()
    {
        var mock = new Mock<ICvStorageService>();

        mock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync((string userId, Stream _, string _2) => $"{userId}/cv.pdf");

        mock.Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return mock;
    }
}
