using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class LoginDialog
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] private IModelValidator<LoginModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private readonly LoginModel _model = new();
    private string ErrorMessage { get; set; } = string.Empty;

    private InputType _passwordInputType = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

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

    private void OnUsernameChanged(string value)
    {
        _model.UserName = value;
        ErrorMessage = string.Empty;
        ModelValidator.ResetErrors(nameof(LoginModel.UserName));
    }

    private void OnPasswordChanged(string value)
    {
        _model.Password = value;
        ErrorMessage = string.Empty;
        ModelValidator.ResetErrors(nameof(LoginModel.Password));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private async Task SubmitAsync()
    {
        if (!ModelValidator.Validate(_model)) return;

        var response = await AuthenticationService.AuthenticateAsync(_model);

        if (!response.Success)
        {
            ErrorMessage = !string.IsNullOrEmpty(response.ValidationErrors)
                ? response.ValidationErrors
                : response.ErrorMessage;
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private async Task OpenRegisterDialogAsync()
    {
        MudDialog.Close();
        var options = new DialogOptions { BackdropClick = false };
        var dialog = await DialogService.ShowAsync<RegisterDialog>("", options);
        await dialog.Result;

        var loginOptions = new DialogOptions { BackdropClick = false };
        await DialogService.ShowAsync<LoginDialog>("", loginOptions);
    }
}
