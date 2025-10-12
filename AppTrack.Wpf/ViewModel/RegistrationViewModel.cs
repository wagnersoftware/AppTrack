using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel;

public partial class RegistrationViewModel : AppTrackFormViewModelBase<RegistrationModel>
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public RegistrationViewModel(IModelValidator<RegistrationModel> modelValidator, RegistrationModel model, IAuthenticationService authenticationService) : base(modelValidator, model)
    {
        this._authenticationService = authenticationService;
    }

    protected override async Task Save(Window window)
    {
        ErrorMessage = string.Empty;

        if (_modelValidator.Validate(Model) == false)
        {
            OnPropertyChanged(nameof(Errors));
            return;
        }

        var apiResponse = await _authenticationService.RegisterAsync(Model);

        if (apiResponse.Success == false)
        {
            ErrorMessage = apiResponse.ValidationErrors;
            return;
        }

        await base.SaveWithoutValidation(window);
    }
}
