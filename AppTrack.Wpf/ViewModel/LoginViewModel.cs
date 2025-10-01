using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppTrack.WpfUi.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    private readonly IModelValidator<LoginModel> _modelValidator;

    public IReadOnlyDictionary<string, List<string>> Errors => _modelValidator.Errors;

    public event Action? LoginSucceeded;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginModel Model { get; }

    public LoginViewModel(LoginModel model, IAuthenticationService authenticationService, IModelValidator<LoginModel> modelValidator)
    {
        this.Model = model;
        this._authenticationService = authenticationService;
        this._modelValidator = modelValidator;
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
        }

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


