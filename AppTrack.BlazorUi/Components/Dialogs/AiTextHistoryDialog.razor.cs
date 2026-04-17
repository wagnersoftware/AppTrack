using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class AiTextHistoryDialog
{
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public JobApplicationModel JobApplication { get; set; } = null!;

    private async Task CopyTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        ErrorHandlingService.ShowSuccess("Copied to clipboard.");
    }

    private void Close() => MudDialog.Close();
}
