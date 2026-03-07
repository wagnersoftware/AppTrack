using AppTrack.BlazorUi.Components.Dialogs;
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class Home : IDisposable
{
    [Inject] private IJobApplicationService JobApplicationService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;

    private static readonly DialogOptions _dialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
    };

    private static readonly DialogOptions _generateTextDialogOptions = new()
    {
        BackdropClick = false,
        MaxWidth = MaxWidth.Large,
        FullWidth = true,
    };

    private MudMessageBox? _deleteConfirmBox;
    private string _deleteConfirmMessage = string.Empty;

    private List<JobApplicationModel> _jobApplications = [];
    private JobApplicationModel.JobApplicationStatus? _selectedStatus;
    private string _searchText = string.Empty;
    // True while the initial job-application list is being fetched; replaces the card grid with a toolbar spinner.
    private bool _isLoading;

    private IEnumerable<JobApplicationModel> _filteredJobApplications =>
        _jobApplications
            .Where(x => _selectedStatus is not { } s || x.Status == s)
            .Where(x => string.IsNullOrWhiteSpace(_searchText)
                        || x.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                        || x.Position.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                        || x.Location.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

    private int _filteredCount => _filteredJobApplications.Count();

    protected override async Task OnInitializedAsync()
    {
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthStateChanged;

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
            await LoadJobApplicationsAsync();
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> authStateTask)
    {
        var authState = await authStateTask;

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            await LoadJobApplicationsAsync();
        }
        else
        {
            _jobApplications = [];
            _selectedStatus = null;
            _searchText = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadJobApplicationsAsync()
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);

        var response = await JobApplicationService.GetJobApplicationsForUserAsync();
        _jobApplications = response.Success ? response.Data ?? [] : [];

        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void OnStatusFilterChanged(JobApplicationModel.JobApplicationStatus? status) =>
        _selectedStatus = status;

    private void OnSearchChanged(string value) =>
        _searchText = value ?? string.Empty;

    private async Task CreateJobApplicationAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateJobApplicationDialog>("", _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not JobApplicationModel newModel) return;

        _jobApplications.Add(newModel);
        await InvokeAsync(StateHasChanged);
    }

    private async Task EditJobApplicationAsync(JobApplicationModel model)
    {
        var parameters = new DialogParameters<EditJobApplicationDialog>
        {
            { x => x.JobApplication, model }
        };

        var dialog = await DialogService.ShowAsync<EditJobApplicationDialog>("", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not JobApplicationModel updatedModel) return;

        var index = _jobApplications.IndexOf(model);
        if (index >= 0)
            _jobApplications[index] = updatedModel;

        await InvokeAsync(StateHasChanged);
    }

    private async Task GenerateTextAsync(JobApplicationModel model)
    {
        var parameters = new DialogParameters<GenerateTextDialog>
        {
            { x => x.JobApplication, model }
        };

        var dialog = await DialogService.ShowAsync<GenerateTextDialog>("", parameters, _generateTextDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: true }) return;
        if (result?.Data is not string generatedText) return;

        model.ApplicationText = generatedText;
        await InvokeAsync(StateHasChanged);
    }

    private async Task DeleteJobApplicationAsync(JobApplicationModel model)
    {
        _deleteConfirmMessage = $"Are you sure you want to delete '{model.Name}'?";

        var confirmed = await _deleteConfirmBox!.ShowAsync();
        if (confirmed != true)
            return;

        var response = await JobApplicationService.DeleteJobApplicationAsync(model.Id);

        if (!ErrorHandlingService.HandleResponse(response)) return;
        if (response.Data is null) return;

        _jobApplications.Remove(model);
        await InvokeAsync(StateHasChanged);
    }

    internal static Color GetStatusColor(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => Color.Primary,
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => Color.Warning,
        JobApplicationModel.JobApplicationStatus.Rejected => Color.Default,
        _ => Color.Default
    };

    internal static string GetStatusHexColor(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => "#0078D4",
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => "#EF6C00",
        JobApplicationModel.JobApplicationStatus.Rejected => "#546E7A",
        _ => "#9E9E9E"
    };

    internal static string GetStatusLabel(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => "New",
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => "Waiting",
        JobApplicationModel.JobApplicationStatus.Rejected => "Rejected",
        _ => status.ToString()
    };

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
