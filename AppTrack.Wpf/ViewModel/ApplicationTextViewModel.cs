using AppTrack.Frontend.Models;
using AppTrack.WpfUi.Helpers;

namespace AppTrack.WpfUi.ViewModel;

public class ApplicationTextViewModel : TextViewModel
{
    public ApplicationTextViewModel(ApplicationTextModel textModel) : base(textModel)
    {
        base.UserMessage = UserMessageHelper.GenerateUserMessageforUnusedKeys(textModel.UnusedKeys);
    }
}
