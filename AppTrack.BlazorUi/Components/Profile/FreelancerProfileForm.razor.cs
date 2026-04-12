using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Profile;

public partial class FreelancerProfileForm
{
    [Parameter] public FreelancerProfileModel Model { get; set; } = new();
    [Parameter] public EventCallback<bool> OnCvBusyChanged { get; set; }
    [Inject] private IModelValidator<FreelancerProfileModel> ModelValidator { get; set; } = null!;
    [Inject] private IFreelancerProfileService ProfileService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private string _selectedType = "Freelancer";
    private DateTime? _availableFrom;
    private bool _cvBusy;
    private MudMessageBox? _cvDeleteConfirmBox;

    protected override void OnParametersSet()
    {
        _availableFrom = Model.AvailableFrom.HasValue
            ? Model.AvailableFrom.Value.ToDateTime(TimeOnly.MinValue)
            : null;
    }

    public bool Validate() => ModelValidator.Validate(Model);

    private string GetFirstError(string propertyName)
    {
        var errors = ModelValidator.Errors.GetValueOrDefault(propertyName);
        return errors is { Count: > 0 } ? errors[0] : string.Empty;
    }

    private void SelectFreelancer() => _selectedType = "Freelancer";

    private void OnFirstNameChanged(string value)
    {
        Model.FirstName = value;
        ModelValidator.ResetErrors(nameof(Model.FirstName));
    }

    private void OnLastNameChanged(string value)
    {
        Model.LastName = value;
        ModelValidator.ResetErrors(nameof(Model.LastName));
    }

    private void OnAvailableFromChanged(DateTime? date)
    {
        _availableFrom = date;
        Model.AvailableFrom = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    }

    private void OnRateKindChanged(RateKind? newKind)
    {
        if (Model.SelectedRateType == newKind) return;
        Model.SelectedRateType = newKind;
        if (newKind == RateKind.Hourly)
        {
            Model.DailyRate = null;
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
        else if (newKind == RateKind.Daily)
        {
            Model.HourlyRate = null;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
        }
        else
        {
            Model.HourlyRate = null;
            Model.DailyRate = null;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
    }

    // Only called while SelectedRateType != null (enforced by the @if guard in the markup).
    private void OnRateValueChanged(decimal? value)
    {
        if (Model.SelectedRateType == RateKind.Hourly)
        {
            Model.HourlyRate = value;
            ModelValidator.ResetErrors(nameof(Model.HourlyRate));
        }
        else if (Model.SelectedRateType == RateKind.Daily)
        {
            Model.DailyRate = value;
            ModelValidator.ResetErrors(nameof(Model.DailyRate));
        }
    }

    private void OnSkillsChanged(string? value)
    {
        Model.Skills = value;
        ModelValidator.ResetErrors(nameof(Model.Skills));
    }

    private string GetCardStyle(string type) =>
        _selectedType == type
            ? "cursor: pointer; border: 2px solid var(--mud-palette-primary);"
            : "cursor: pointer;";

    private async Task OnCvFileChanged(IBrowserFile file)
    {
        _cvBusy = true;
        await OnCvBusyChanged.InvokeAsync(true);

        try
        {
            var response = await ProfileService.UploadCvAsync(file);

            if (response.Success)
            {
                Model.CvFileName = response.Data?.CvFileName;
                Snackbar.Add("CV uploaded successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add(response.DisplayMessage, Severity.Error);
            }
        }
        finally
        {
            _cvBusy = false;
            await OnCvBusyChanged.InvokeAsync(false);
        }
    }

    private async Task OnCvDeleteClicked()
    {
        var confirmed = await _cvDeleteConfirmBox!.ShowAsync();
        if (confirmed != true) return;

        _cvBusy = true;
        await OnCvBusyChanged.InvokeAsync(true);

        try
        {
            var response = await ProfileService.DeleteCvAsync();

            if (response.Success)
            {
                Model.CvFileName = null;
                Snackbar.Add("CV deleted successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add(response.DisplayMessage, Severity.Error);
            }
        }
        finally
        {
            _cvBusy = false;
            await OnCvBusyChanged.InvokeAsync(false);
        }
    }
}
