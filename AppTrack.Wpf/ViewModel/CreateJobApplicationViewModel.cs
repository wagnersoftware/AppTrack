using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class CreateJobApplicationViewModel : AppTrackFormViewModelBase<JobApplicationModel>
{
    public CreateJobApplicationViewModel(IModelValidator<JobApplicationModel> modelValidator)
        : base(modelValidator, new JobApplicationModel())
    {

    }
}
