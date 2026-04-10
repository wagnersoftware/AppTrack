using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
{
    public int SelectedChatModelId { get; set; }
    public AiResponseLanguage Language { get; set; } = AiResponseLanguage.English;
    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
    public ObservableCollection<PromptModel> Prompts { get; set; } = new ObservableCollection<PromptModel>();
    public List<PromptModel> DefaultPrompts { get; set; } = [];

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameter => PromptParameter;
    IEnumerable<IPromptValidatable> IAiSettingsValidatable.Prompts => Prompts;
}
