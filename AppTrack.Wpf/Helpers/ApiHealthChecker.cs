using AppTrack.WpfUi.Contracts;
using AppTrack.WpfUi.MessageBoxService;
using System.Net.Http;

namespace AppTrack.WpfUi.Helpers;

public class ApiHealthChecker : IApiHealthChecker
{
    private readonly IMessageBoxService _messageBoxService;

    public ApiHealthChecker(IMessageBoxService messageBoxService)
    {
        _messageBoxService = messageBoxService;
    }
    public async Task<bool> WaitForBackendAsync(string url, int retryCount)
    {
        using var client = new HttpClient();

        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch(Exception e)
            {
                _messageBoxService.ShowErrorMessageBox("Error in healthcheck: " + e.Message);
            }

            await Task.Delay(500);
        }
        _messageBoxService.ShowErrorMessageBox("Api health check timed out!");
        return false;
    }
}
