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

    private List<JobApplicationModel> _jobApplications = [];
    private bool _isLoading;

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

    internal static Color GetStatusColor(JobApplicationModel.JobApplicationStatus status) => status switch
    {
        JobApplicationModel.JobApplicationStatus.New => Color.Primary,
        JobApplicationModel.JobApplicationStatus.WaitingForFeedback => Color.Warning,
        JobApplicationModel.JobApplicationStatus.Rejected => Color.Error,
        _ => Color.Default
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
