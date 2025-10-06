using AppTrack.Frontend.Models;
using AppTrack.WpfUi.WindowService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel;

public partial class EditPromptParameterViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<KeyValueItemModel> PromptParameter { get; set; }

    public EditPromptParameterViewModel(ObservableCollection<KeyValueItemModel> promptParameter, IWindowService windowService, IServiceProvider serviceProvider)
    {
        this.PromptParameter = promptParameter;
        this._windowService = windowService;
        this._serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private void AddKeyValueItem()
    {
        var editKeyItemViewModel = _serviceProvider.GetRequiredService<EditKeyValueItemViewModel>();
        var dialogResult = _windowService.ShowWindow(editKeyItemViewModel);

        if (dialogResult == true)
        {
            PromptParameter.Add(editKeyItemViewModel.Model);
        }
    }

    [RelayCommand]
    private void EditKeyValueItem(KeyValueItemModel keyValueItem)
    {
        var clone = keyValueItem.Clone();

        var editKeyItemViewModel = ActivatorUtilities.CreateInstance<EditKeyValueItemViewModel>(_serviceProvider, clone);
        var dialogResult = _windowService.ShowWindow(editKeyItemViewModel);

        if(dialogResult == true)
        {
            keyValueItem.Key = clone.Key;
            keyValueItem.Value = clone.Value;
        }
    }

    [RelayCommand]
    private void DeleteKeyValueItem(int id)
    {
        var itemToRemove = PromptParameter.FirstOrDefault(x => x.Id == id);

        if (itemToRemove != null)
        {
            PromptParameter.Remove(itemToRemove);
        }
    }

    [RelayCommand]
    private void Save(Window window)
    {
        window.DialogResult = true;
        window.Close();
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }
}
