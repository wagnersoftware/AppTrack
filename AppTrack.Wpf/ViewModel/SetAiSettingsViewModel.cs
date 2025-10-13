using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

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
    private void AddPromptParameter()
    {
        var keyValueItem = new PromptParameterModel() { ParentCollection = this.Model.PromptParameter };
        var editKeyItemViewModel = ActivatorUtilities.CreateInstance<EditKeyValueItemViewModel>(_serviceProvider, keyValueItem);
        var dialogResult = _windowService.ShowWindow(editKeyItemViewModel);

        if (dialogResult == true)
        {
            Model.PromptParameter.Add(editKeyItemViewModel.Model);
        }
    }

    [RelayCommand]
    private void EditPromptParameter(PromptParameterModel keyValueItem)
    {
        var clone = keyValueItem.Clone();
        clone.ParentCollection = this.Model.PromptParameter;

        var editKeyItemViewModel = ActivatorUtilities.CreateInstance<EditKeyValueItemViewModel>(_serviceProvider, clone);
        var dialogResult = _windowService.ShowWindow(editKeyItemViewModel);

        if (dialogResult == true)
        {
            keyValueItem.Key = clone.Key;
            keyValueItem.Value = clone.Value;
        }
    }

    [RelayCommand]
    private void DeletePromptParameter(Guid tempId)
    {
        var itemToRemove = Model.PromptParameter.FirstOrDefault(x => x.TempId == tempId);

        if (itemToRemove != null)
        {
            Model.PromptParameter.Remove(itemToRemove);
        }
    }
}
