using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTrack.WpfUi.ViewModel.Base;

public partial class JobApplicationViewModelBase : AppTrackFormViewModelBase<JobApplicationModel>
{
    [ObservableProperty]
    private bool isEditView = false;

    [ObservableProperty]
    private string windowTitle = string.Empty;

    public JobApplicationViewModelBase(IModelValidator<JobApplicationModel> modelValidator, JobApplicationModel model) : base(modelValidator, model)
    {

    }
}
