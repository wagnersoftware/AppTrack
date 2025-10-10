using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class CreateJobApplicationViewModel : AppTrackFormViewModelBase<JobApplicationModel>
{
    public CreateJobApplicationViewModel(IModelValidator<JobApplicationModel> modelValidator, JobApplicationModel model)
        : base(modelValidator, model)
    {
    }

    public void SetDefaults(JobApplicationDefaultsModel jobApplicationDefaults)
    {
        base.Model.Name = jobApplicationDefaults.Name;
        base.Model.Position = jobApplicationDefaults.Position;
        base.Model.StartDate = DateOnly.FromDateTime(DateTime.Today);
    }
}
