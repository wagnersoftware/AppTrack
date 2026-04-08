using AppTrack.Application.Contracts.CvStorage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace AppTrack.Infrastructure.CvStorage;

public class AzureBlobStorageService : ICvStorageService
{
    private readonly AzureStorageSettings _settings;

    public AzureBlobStorageService(IOptions<AzureStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<string> UploadAsync(string userId, Stream stream, string fileName)
    {
        var blobPath = $"{userId}/cv.pdf";

        var serviceClient = new BlobServiceClient(_settings.ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_settings.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobPath);
        await blobClient.UploadAsync(stream, overwrite: true);

        return blobPath;
    }

    public async Task DeleteAsync(string blobPath)
    {
        var serviceClient = new BlobServiceClient(_settings.ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_settings.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync();
    }
}
