using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class CreateJobApplicationViewModel : JobApplicationViewModelBase
{
    public CreateJobApplicationViewModel(IModelValidator<JobApplicationModel> modelValidator, JobApplicationModel model)
        : base(modelValidator, model)
    {
        base.IsEditView = false;
        base.WindowTitle = "Create Job Application";
    }

    public void SetDefaults(JobApplicationDefaultsModel jobApplicationDefaults)
    {
        base.Model.Name = jobApplicationDefaults.Name;
        base.Model.Position = jobApplicationDefaults.Position;
        base.Model.StartDate = DateOnly.FromDateTime(DateTime.Today);
    }
}
