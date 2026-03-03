using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class RegisterDialog
{
    [Inject] private IModelValidator<RegistrationModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private readonly RegistrationModel _model = new();
    private string ErrorMessage { get; set; } = string.Empty;

    private void OnUsernameChanged(string value)
    {
        _model.UserName = value;
        ModelValidator.ResetErrors(nameof(RegistrationModel.UserName));
    }

    private void OnPasswordChanged(string value)
    {
        _model.Password = value;
        ModelValidator.ResetErrors(nameof(RegistrationModel.Password));
    }

    private void OnConfirmPasswordChanged(string value)
    {
        _model.ConfirmPassword = value;
        ModelValidator.ResetErrors(nameof(RegistrationModel.ConfirmPassword));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private void Submit()
    {
        if (!ModelValidator.Validate(_model)) return;
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel() => MudDialog.Cancel();
}
