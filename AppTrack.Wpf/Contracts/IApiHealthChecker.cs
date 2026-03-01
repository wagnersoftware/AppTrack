namespace AppTrack.WpfUi.Contracts;

internal interface IApiHealthChecker
{
    Task<bool> WaitForBackendAsync(string url, int retryCount);
}
