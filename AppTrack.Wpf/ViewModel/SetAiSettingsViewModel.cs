using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace AppTrack.WpfUi.ViewModel;

public partial class SetAiSettingsViewModel : AppTrackFormViewModelBase<AiSettingsModel>
{
    private readonly IWindowService _windowService;
    private readonly IServiceProvider _serviceProvider;

    public SetAiSettingsViewModel(IModelValidator<AiSettingsModel> modelValidator, AiSettingsModel model, IWindowService windowService, IServiceProvider serviceProvider) : base(modelValidator, model)
    {
        this._windowService = windowService;
        this._serviceProvider = serviceProvider;
    }

    [RelayCommand]
    public void EditPromptParameter()
    {
        ObservableCollection<PromptParameterModel> observableList = new ObservableCollection<PromptParameterModel>(Model.PromptParameter);
        var viewModel = ActivatorUtilities.CreateInstance<EditPromptParameterViewModel>(_serviceProvider, observableList);

        var dialogResult =_windowService.ShowWindow(viewModel);

        if (dialogResult == true)
        {
            Model.PromptParameter = [.. viewModel.PromptParameter];
        }
    }
}
