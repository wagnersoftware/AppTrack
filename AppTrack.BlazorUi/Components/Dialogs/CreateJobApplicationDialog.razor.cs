using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class CreateJobApplicationDialog
{
    [Inject] private IJobApplicationService JobApplicationService { get; set; } = null!;
    [Inject] private IModelValidator<JobApplicationModel> ModelValidator { get; set; } = null!;
    [Inject] private ISnackbarService SnackbarService { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private readonly JobApplicationModel _model = new();
    private DateTime? _startDate;
    // True only during the create API call; keeps the form visible but disables action buttons and shows an inline spinner.
    private bool _isBusy;

    private void OnNameChanged(string value)
    {
        _model.Name = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.Name));
    }

    private void OnPositionChanged(string value)
    {
        _model.Position = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.Position));
    }

    private void OnUrlChanged(string value)
    {
        _model.URL = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.URL));
    }

    private void OnJobDescriptionChanged(string value)
    {
        _model.JobDescription = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.JobDescription));
    }

    private void OnLocationChanged(string value)
    {
        _model.Location = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.Location));
    }

    private void OnContactPersonChanged(string value)
    {
        _model.ContactPerson = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.ContactPerson));
    }

    private void OnDurationChanged(string value)
    {
        _model.DurationInMonths = value;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.DurationInMonths));
    }

    private void OnStartDateChanged(DateTime? date)
    {
        _startDate = date;
        _model.StartDate = date.HasValue ? DateOnly.FromDateTime(date.Value) : DateOnly.MinValue;
        ModelValidator.ResetErrors(nameof(JobApplicationModel.StartDate));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private async Task SubmitAsync()
    {
        if (!ModelValidator.Validate(_model)) return;

        _isBusy = true;
        var response = await JobApplicationService.CreateJobApplicationForUserAsync(_model);
        _isBusy = false;

        if (!SnackbarService.HandleResponse(response)) return;
        if (response.Data is null) return;

        MudDialog.Close(DialogResult.Ok(response.Data));
    }

    private void Cancel() => MudDialog.Cancel();
}
