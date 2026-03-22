using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class GenerateTextDialog : IDisposable
{
    [Inject] private IApplicationTextService ApplicationTextService { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public JobApplicationModel JobApplication { get; set; } = null!;
    [Parameter] public List<string> PromptNames { get; set; } = [];

    private enum Phase { LoadingPrompt, PromptReady, GeneratingText, TextReady }

    private Phase _phase = Phase.LoadingPrompt;
    private string _prompt = string.Empty;
    private string _generatedText = string.Empty;
    private List<string> _unusedKeys = [];
    private CancellationTokenSource _cts = new();
    private string _selectedPromptName = string.Empty;
    private bool _isReloadingPrompt;

    protected override async Task OnInitializedAsync()
    {
        _selectedPromptName = PromptNames[0];
        var response = await ApplicationTextService.GeneratePrompt(JobApplication.Id, _selectedPromptName);

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
        {
            MudDialog.Cancel();
            return;
        }

        _prompt = response.Data.Text;
        _unusedKeys = response.Data.UnusedKeys;
        _phase = Phase.PromptReady;
    }

    private async Task OnPromptNameChangedAsync(string newName)
    {
        _selectedPromptName = newName;
        _isReloadingPrompt = true;
        StateHasChanged();

        var response = await ApplicationTextService.GeneratePrompt(JobApplication.Id, newName);
        _isReloadingPrompt = false;

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
            return;

        _prompt = response.Data.Text;
        _unusedKeys = response.Data.UnusedKeys;
    }

    private async Task SendPromptAsync()
    {
        _phase = Phase.GeneratingText;
        _cts = new CancellationTokenSource();

        var response = await ApplicationTextService.GenerateApplicationText(_prompt, JobApplication.Id, _cts.Token);

        if (_cts.IsCancellationRequested)
        {
            _phase = Phase.PromptReady;
            return;
        }

        if (!ErrorHandlingService.HandleResponse(response) || response.Data is null)
        {
            _phase = Phase.PromptReady;
            return;
        }

        _generatedText = response.Data.Text;
        _phase = Phase.TextReady;
    }

    private void AbortGeneration() => _cts.Cancel();

    private async Task CopyTextAsync()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", _generatedText);
        ErrorHandlingService.ShowSuccess("Copied to clipboard.");
    }

    private async Task CopyAndCloseAsync()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", _generatedText);
        MudDialog.Close(DialogResult.Ok(_generatedText));
    }

    private void Cancel() => MudDialog.Cancel();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
