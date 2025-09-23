using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public partial class EditJobApplicationViewModel : AppTrackFormViewModelBase<JobApplicationModel>
{
    public EditJobApplicationViewModel(JobApplicationModel jobApplicationModel, IModelValidator<JobApplicationModel> modelValidator)
        : base(modelValidator, jobApplicationModel)
    {

    }
}
