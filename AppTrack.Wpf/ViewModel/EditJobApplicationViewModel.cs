using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class EditJobApplicationViewModel : JobApplicationViewModelBase
{
    public EditJobApplicationViewModel(JobApplicationModel jobApplicationModel, IModelValidator<JobApplicationModel> modelValidator)
        : base(modelValidator, jobApplicationModel)
    {
        base.IsEditView = true;
        base.WindowTitle = "Edit Job Application";
    }
}
