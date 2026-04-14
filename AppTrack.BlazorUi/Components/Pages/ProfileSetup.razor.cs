using AppTrack.BlazorUi.Components.Profile;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Pages;

public partial class ProfileSetup
{
    [Inject] private IFreelancerProfileService ProfileService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private FreelancerProfileModel _model = new();
    private FreelancerProfileForm _form = null!;
    private bool _isBusy;
    private MudMessageBox _deleteConfirmBox = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileService.GetProfileAsync();
        if (response.Success && response.Data is not null)
        {
            _model = response.Data;
        }
    }

    private async Task ConfirmDeleteProfile()
    {
        var confirmed = await _deleteConfirmBox.ShowAsync();
        if (confirmed != true) return;

        _isBusy = true;
        await InvokeAsync(StateHasChanged);
        var response = await ProfileService.DeleteProfileAsync();
        _isBusy = false;

        if (response.Success)
        {
            _model = new FreelancerProfileModel();
            Snackbar.Add("Profile deleted successfully.", Severity.Success);
        }
        else
        {
            Snackbar.Add(response.DisplayMessage, Severity.Error);
        }
    }

    private async Task Save()
    {
        if (!_form.Validate()) return;

        _isBusy = true;
        await InvokeAsync(StateHasChanged);
        var response = await ProfileService.UpsertProfileAsync(_model);
        _isBusy = false;

        if (response.Success)
        {
            Snackbar.Add("Profile saved", Severity.Success);
        }
        else
        {
            Snackbar.Add(response.DisplayMessage, Severity.Error);
        }
    }
}
