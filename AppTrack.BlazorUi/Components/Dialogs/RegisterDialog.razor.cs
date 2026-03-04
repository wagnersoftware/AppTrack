using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class RegisterDialog
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] private IModelValidator<RegistrationModel> ModelValidator { get; set; } = null!;
    [Inject] private IErrorHandlingService ErrorHandlingService { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private readonly RegistrationModel _model = new();

    internal static readonly Dictionary<string, object> UsernameAttributes = new() { { "autocomplete", "off" } };
    internal static readonly Dictionary<string, object> PasswordAttributes = new() { { "autocomplete", "new-password" } };

    private InputType _passwordInputType = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private InputType _confirmPasswordInputType = InputType.Password;
    private string _confirmPasswordInputIcon = Icons.Material.Filled.VisibilityOff;

    private void TogglePasswordVisibility()
    {
        if (_passwordInputType == InputType.Password)
        {
            _passwordInputType = InputType.Text;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            _passwordInputType = InputType.Password;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
        }
    }

    private void ToggleConfirmPasswordVisibility()
    {
        if (_confirmPasswordInputType == InputType.Password)
        {
            _confirmPasswordInputType = InputType.Text;
            _confirmPasswordInputIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            _confirmPasswordInputType = InputType.Password;
            _confirmPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        }
    }

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

    private async Task SubmitAsync()
    {
        if (!ModelValidator.Validate(_model)) return;

        var response = await AuthenticationService.RegisterAsync(_model);

        if (!ErrorHandlingService.HandleResponse(response)) return;

        ErrorHandlingService.ShowSuccess($"User {_model.UserName} registered successfully.");
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel() => MudDialog.Cancel();
}
