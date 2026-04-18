using AppTrack.Application.Contracts.CvStorage;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

internal sealed class StubCvStorageService : ICvStorageService
{
    public const string FakeBlobPath = "test-user/cv.pdf";

    public Task<string> UploadAsync(string userId, Stream stream, string fileName)
        => Task.FromResult(FakeBlobPath);

    public Task DeleteAsync(string blobPath)
        => Task.CompletedTask;
}
