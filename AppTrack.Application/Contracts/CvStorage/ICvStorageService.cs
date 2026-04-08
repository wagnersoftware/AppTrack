namespace AppTrack.Application.Contracts.CvStorage;

public interface ICvStorageService
{
    /// <summary>Uploads the stream as {userId}/cv.pdf and returns the blob path.</summary>
    Task<string> UploadAsync(string userId, Stream stream, string fileName);

    /// <summary>Deletes the blob at the given path. No-op if it does not exist.</summary>
    Task DeleteAsync(string blobPath);
}
