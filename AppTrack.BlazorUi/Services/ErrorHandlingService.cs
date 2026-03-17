using AppTrack.Frontend.ApiService.Base;
using MudBlazor;

namespace AppTrack.BlazorUi.Services;

public class ErrorHandlingService(ISnackbar snackbar) : IErrorHandlingService
{
    public void ShowError(string message) =>
        snackbar.Add(message, Severity.Error);

    public void ShowSuccess(string message) =>
        snackbar.Add(message, Severity.Success);

    public bool HandleResponse<T>(Response<T> response)
    {
        if (response.Success) return true;

        snackbar.Add(response.DisplayMessage, Severity.Error);
        return false;
    }
}
