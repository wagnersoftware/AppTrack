using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class EditJobApplicationDialog
{
    [Inject] private IJobApplicationService JobApplicationService { get; set; } = null!;
    [Inject] private IModelValidator<JobApplicationModel> ModelValidator { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public JobApplicationModel JobApplication { get; set; } = null!;

    private JobApplicationModel _model = null!;
    private DateTime? _startDate;
    // True only during the update API call; keeps the form visible but disables action buttons and shows an inline spinner.
    private bool _isBusy;

    protected override void OnParametersSet()
    {
        _model = new JobApplicationModel
        {
            Id = JobApplication.Id,
            Name = JobApplication.Name,
            Position = JobApplication.Position,
            URL = JobApplication.URL,
            JobDescription = JobApplication.JobDescription,
            Location = JobApplication.Location,
            ContactPerson = JobApplication.ContactPerson,
            Status = JobApplication.Status,
            StartDate = JobApplication.StartDate,
            DurationInMonths = JobApplication.DurationInMonths,
            ApplicationText = JobApplication.ApplicationText,
            CreationDate = JobApplication.CreationDate,
            ModifiedDate = JobApplication.ModifiedDate,
        };

        _startDate = JobApplication.StartDate != DateOnly.MinValue
            ? JobApplication.StartDate.ToDateTime(TimeOnly.MinValue)
            : null;
    }

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

    private void OnStatusChanged(JobApplicationModel.JobApplicationStatus value)
    {
        _model.Status = value;
    }

    private void OnApplicationTextChanged(string value)
    {
        _model.ApplicationText = value;
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private async Task SubmitAsync()
    {
        if (!ModelValidator.Validate(_model)) return;

        _isBusy = true;
        var response = await JobApplicationService.UpdateJobApplicationAsync(_model.Id, _model);
        _isBusy = false;

        if (!ErrorHandlingService.HandleResponse(response)) return;
        if (response.Data is null) return;

        MudDialog.Close(DialogResult.Ok(response.Data));
    }

    private async Task CopyApplicationTextAsync()
    {
        if (string.IsNullOrEmpty(_model.ApplicationText)) return;
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", _model.ApplicationText);
        Snackbar.Add("Copied to clipboard", Severity.Success);
    }

    private void Cancel() => MudDialog.Cancel();
}
