using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel.Base;

public partial class AppTrackFormViewModelBase<T> : ObservableObject where T : ModelBase
{
    public T Model { get; }

    protected readonly IModelValidator<T> _modelValidator;

    public IReadOnlyDictionary<string, List<string>> Errors => _modelValidator.Errors;

    public AppTrackFormViewModelBase(IModelValidator<T> modelValidator, T model)
    {
        Model = model ?? throw new ArgumentNullException("Null is not valid for the model, pass a new instance of Type T instead");
        this._modelValidator = modelValidator;
    }

    [RelayCommand]
    protected virtual async Task Save(Window window)
    {
        await SaveInternal(window, false);
    }

    [RelayCommand]
    protected virtual async Task SaveWithoutValidation(Window window)
    {
        await SaveInternal(window, true);
    }

    [RelayCommand]
    protected virtual async Task Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();

        await Task.CompletedTask;
    }

    [RelayCommand]
    protected virtual void ResetErrors(string propertyName)
    {
        _modelValidator.ResetErrors(propertyName);
        OnPropertyChanged(nameof(Errors));
    }

    private async Task SaveInternal(Window window, bool validate)
    {
        if (validate && !_modelValidator.Validate(Model))
        {
            OnPropertyChanged(nameof(Errors));
            return;
        }

        window.DialogResult = true;
        window.Close();
        await Task.CompletedTask;
    }
}
