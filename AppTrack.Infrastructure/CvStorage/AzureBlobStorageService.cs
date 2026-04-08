using AppTrack.Application.Contracts.CvStorage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace AppTrack.Infrastructure.CvStorage;

public class AzureBlobStorageService : ICvStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IOptions<AzureStorageSettings> settings)
    {
        var serviceClient = new BlobServiceClient(settings.Value.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(settings.Value.ContainerName);
    }

    public async Task<string> UploadAsync(string userId, Stream stream, string fileName)
    {
        var blobPath = $"{userId}/cv.pdf";

        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.UploadAsync(stream, overwrite: true);

        return blobPath;
    }

    public async Task DeleteAsync(string blobPath)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync();
    }
}
