using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
{
    public int SelectedChatModelId { get; set; }
    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
    public ObservableCollection<PromptModel> Prompts { get; set; } = new ObservableCollection<PromptModel>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
    IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
}
