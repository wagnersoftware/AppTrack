using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.WpfUi.ViewModel.Base;

namespace AppTrack.WpfUi.ViewModel;

public class SetAiSettingsViewModel : AppTrackFormViewModelBase<AiSettingsModel>
{
    public SetAiSettingsViewModel(IModelValidator<AiSettingsModel> modelValidator, AiSettingsModel model) : base(modelValidator, model)
    {
    }
}
