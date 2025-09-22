using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AppTrack.WpfUi.ViewModel;

public partial class EditJobApplicationViewModel : ObservableObject
{
    private readonly IModelValidator<JobApplicationModel> _modelValidator;

    public IReadOnlyDictionary<string, List<string>> Errors => _modelValidator.Errors;

    public EditJobApplicationViewModel(JobApplicationModel jobApplicationModel, IModelValidator<JobApplicationModel> modelValidator)
    {
        JobApplication = jobApplicationModel;
        this._modelValidator = modelValidator;
    }

    public JobApplicationModel JobApplication { get; set; } = new();

    [RelayCommand]
    private void Save(Window window)
    {
        if (_modelValidator.Validate(JobApplication) == false)
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
