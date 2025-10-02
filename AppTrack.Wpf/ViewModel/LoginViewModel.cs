using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.CredentialManagement;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppTrack.WpfUi.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    private readonly IModelValidator<LoginModel> _modelValidator;
    private readonly ICredentialManager _credentialManager;

    public IReadOnlyDictionary<string, List<string>> Errors => _modelValidator.Errors;

    public event Action? LoginSucceeded;

    [ObservableProperty]
    private string errorMessage = string.Empty;


    [ObservableProperty]
    private bool isRememberMeChecked;

    [ObservableProperty]
    private bool isPasswordVisible;

    public LoginModel Model { get; }

    public LoginViewModel(LoginModel model, IAuthenticationService authenticationService, IModelValidator<LoginModel> modelValidator, ICredentialManager credentialManager)
    {
        this.Model = model;
        this._authenticationService = authenticationService;
        this._modelValidator = modelValidator;
        this._credentialManager = credentialManager;

        //get persisted value from settins
        IsRememberMeChecked = Properties.Settings.Default.RememberMe;

        if(IsRememberMeChecked == false)
        {
            return;
        }

        //load persisted credentials
        var credentials = _credentialManager.LoadCredentials();
        if (credentials.HasValue)
        {
            Model.UserName = credentials.Value.username!;
            Model.Password = credentials.Value.password!;
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        if (_modelValidator.Validate(Model) == false)
        {
            OnPropertyChanged(nameof(Errors));
            return;
        }

        var isAuthenticated = await _authenticationService.AuthenticateAsync(Model);

        if (isAuthenticated == false)
        {
            ErrorMessage = "Authentication failed!";
            return;
        }

        if (IsRememberMeChecked)
        {
            _credentialManager.SaveCredentials(Model.UserName, Model.Password);
        }
        else
        {
            _credentialManager.DeleteCredentials();
        }

        //persist remember me
        Properties.Settings.Default.RememberMe = IsRememberMeChecked;
        Properties.Settings.Default.Save();

        LoginSucceeded?.Invoke();
    }

    [RelayCommand]
    private async Task Register()
    {

    }

    [RelayCommand]
    private void ResetErrors(string propertyName)
    {
        ErrorMessage = string.Empty;
        _modelValidator.ResetErrors(propertyName);
        OnPropertyChanged(nameof(Errors));
    }
}


