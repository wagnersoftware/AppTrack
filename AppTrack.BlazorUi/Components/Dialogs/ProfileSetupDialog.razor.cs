using AppTrack.BlazorUi.Components.Profile;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class ProfileSetupDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IFreelancerProfileService ProfileService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private FreelancerProfileModel _model = new();
    private FreelancerProfileForm _form = null!;
    private bool _isBusy;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileService.GetProfileAsync();
        if (response.Success && response.Data is not null)
        {
            _model = response.Data;
        }
    }

    private async Task Save()
    {
        if (!_form.Validate()) return;

        _isBusy = true;
        var response = await ProfileService.UpsertProfileAsync(_model);
        _isBusy = false;

        if (response.Success)
        {
            MudDialog.Close();
        }
        else
        {
            Snackbar.Add(response.DisplayMessage, Severity.Error);
        }
    }

    private void Skip() => MudDialog.Cancel();
}
