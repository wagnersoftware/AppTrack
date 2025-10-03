using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.CredentialManagement;
using AppTrack.WpfUi.ViewModel.Base;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel;

public partial class LoginViewModel : AppTrackFormViewModelBase<LoginModel>
{
    private readonly IAuthenticationService _authenticationService;

    private readonly ICredentialManager _credentialManager;
    private readonly IWindowService _windowService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string errorMessage = string.Empty;


    [ObservableProperty]
    private bool isRememberMeChecked;

    [ObservableProperty]
    private bool isPasswordVisible;

    public LoginViewModel(
        LoginModel model,
        IAuthenticationService authenticationService,
        IModelValidator<LoginModel> modelValidator,
        ICredentialManager credentialManager,
        IWindowService windowService,
        IServiceProvider serviceProvider) :base(modelValidator, model)
    {
        this._authenticationService = authenticationService;
        this._credentialManager = credentialManager;
        this._windowService = windowService;
        this._serviceProvider = serviceProvider;

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

    protected override async Task Save(Window window)
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

        await base.SaveWithoutValidation(window);
    }

    [RelayCommand]
    private void Register()
    {
        var registrationViewModel = _serviceProvider.GetRequiredService<RegistrationViewModel>();
        _windowService.ShowWindow(registrationViewModel);
    }

    protected override void ResetErrors(string propertyName)
    {
        ErrorMessage = string.Empty;
        base.ResetErrors(propertyName);
    }
}


