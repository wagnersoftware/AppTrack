using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AppTrack.BlazorUi.Components.Dialogs;

public partial class LoginDialog
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IModelValidator<LoginModel> ModelValidator { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private readonly LoginModel _model = new();
    private bool _rememberMe;
    private string ErrorMessage { get; set; } = string.Empty;

    private void OnUsernameChanged(string value)
    {
        _model.UserName = value;
        ModelValidator.ResetErrors(nameof(LoginModel.UserName));
    }

    private void OnPasswordChanged(string value)
    {
        _model.Password = value;
        ModelValidator.ResetErrors(nameof(LoginModel.Password));
    }

    private string GetFirstError(string propertyName)
        => ModelValidator.Errors.GetValueOrDefault(propertyName)?.FirstOrDefault() ?? string.Empty;

    private void Submit()
    {
        if (!ModelValidator.Validate(_model)) return;
        MudDialog.Close(DialogResult.Ok(true));
    }

    private async Task OpenRegisterDialogAsync()
    {
        MudDialog.Close();
        var options = new DialogOptions { BackdropClick = false };
        await DialogService.ShowAsync<RegisterDialog>("", options);
    }
}
