using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.Cache;
using AppTrack.WpfUi.ViewModel.Base;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel;

public partial class SetAiSettingsViewModel : AppTrackFormViewModelBase<AiSettingsModel>
{
    private readonly IWindowService _windowService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private IReadOnlyList<ChatModel> chatModels;

    [ObservableProperty]
    private ChatModel selectedChatModel = new ChatModel();

    public SetAiSettingsViewModel(
            IModelValidator<AiSettingsModel> modelValidator,
            AiSettingsModel model,
            IWindowService windowService,
            IServiceProvider serviceProvider,
            IChatModelStore chatModelStore) : base(modelValidator, model)
    {
        _windowService = windowService;
        _serviceProvider = serviceProvider;
        chatModels = chatModelStore.ChatModels;

        if (ChatModels?.Count > 0)
        {
            //set default 
            if(Model.SelectedChatModelId == 0)
            {
                Model.SelectedChatModelId = 1;
            }
            SelectedChatModel = ChatModels.SingleOrDefault(x => x.Id == Model.SelectedChatModelId) ?? new ChatModel();            
        }
    }

    protected override async Task Save(Window window)
    {
        Model.SelectedChatModelId = SelectedChatModel.Id;
        await base.Save(window);
    }

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
