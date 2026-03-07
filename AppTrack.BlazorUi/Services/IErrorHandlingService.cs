using AppTrack.Frontend.ApiService.Base;

namespace AppTrack.BlazorUi.Services;

public interface IErrorHandlingService
{
    void ShowError(string message);
    void ShowSuccess(string message);

    /// <summary>
    /// Shows an error snackbar when the response was not successful.
    /// Returns true on success, false on error.
    /// </summary>
    bool HandleResponse<T>(Response<T> response);
}
