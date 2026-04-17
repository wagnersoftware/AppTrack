using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class AiTextHistoryDialog
{
    [Inject] private IApplicationTextService ApplicationTextService { get; set; } = null!;
    [Inject] private IJobApplicationService JobApplicationService { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public JobApplicationModel JobApplication { get; set; } = null!;

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var response = await JobApplicationService.GetJobApplicationByIdAsync(JobApplication.Id);
        _isLoading = false;

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
        {
            MudDialog.Cancel();
            return;
        }

        JobApplication.AiTextHistory = response.Data.AiTextHistory;
    }

    private async Task CopyTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        ErrorHandlingService.ShowSuccess("Copied to clipboard.");
    }

    private async Task DeleteEntryAsync(JobApplicationAiTextModel entry)
    {
        var response = await ApplicationTextService.DeleteAiTextAsync(entry.Id);
        if (!ErrorHandlingService.HandleResponse(response)) return;

        JobApplication.AiTextHistory.Remove(entry);
        StateHasChanged();
    }

    private void Close() => MudDialog.Close();
}
