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
        ObservableCollection<KeyValueItemModel> models = new ObservableCollection<KeyValueItemModel>();
        var viewModel = ActivatorUtilities.CreateInstance<EditPromptParameterViewModel>(_serviceProvider, models);

        _windowService.ShowWindow(viewModel);
    }
}
