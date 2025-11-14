using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.Cache;
using AppTrack.WpfUi.ViewModel.Base;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.WpfUi.ViewModel;

public partial class SetAiSettingsViewModel(
    IModelValidator<AiSettingsModel> modelValidator,
    AiSettingsModel model,
    IWindowService windowService,
    IServiceProvider serviceProvider, 
    IChatModelStore chatModelStore) : AppTrackFormViewModelBase<AiSettingsModel>(modelValidator, model)
{
    private readonly IWindowService _windowService = windowService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [ObservableProperty]
    private IReadOnlyList<ChatModel> chatModels = chatModelStore.ChatModels;

    [ObservableProperty]
    private ChatModel selectedChatModel = new ChatModel();

    [RelayCommand]
    private void AddPromptParameter()
    {
        var keyValueItem = new PromptParameterModel() { ParentCollection = Model.PromptParameter };
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
        clone.ParentCollection = Model.PromptParameter;

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
