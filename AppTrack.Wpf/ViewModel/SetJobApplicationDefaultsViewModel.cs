using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public partial class SetJobApplicationDefaultsViewModel : AppTrackFormViewModelBase<JobApplicationDefaultsModel>
{
    public SetJobApplicationDefaultsViewModel(JobApplicationDefaultsModel jobApplicationDefaults, IModelValidator<JobApplicationDefaultsModel> modelValidator) 
        : base(modelValidator, jobApplicationDefaults)
    {

    }
}
