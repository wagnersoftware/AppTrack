using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel.Base;

public partial class AppTrackFormViewModelBase<T>: ObservableObject where T : ModelBase
{
    public T Model { get; set; }

    private readonly IModelValidator<T> _modelValidator;

    public IReadOnlyDictionary<string, List<string>> Errors => _modelValidator.Errors;

    public AppTrackFormViewModelBase(IModelValidator<T> modelValidator, T model)
    {
        Model = model ?? throw new ArgumentNullException("Null is not valid for the model, pass a new instance of Type T instead");
        this._modelValidator = modelValidator;
    }

    [RelayCommand]
    private void Save(Window window)
    {
        if (_modelValidator.Validate(Model) == false)
        {
            OnPropertyChanged(nameof(Errors));
            return;
        }

        window.DialogResult = true;
        window.Close();
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }

    [RelayCommand]
    private void ResetErrors(string propertyName)
    {
        _modelValidator.ResetErrors(propertyName);
        OnPropertyChanged(nameof(Errors));
    }
}
