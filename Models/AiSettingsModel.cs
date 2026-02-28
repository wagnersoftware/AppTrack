using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase, IAiSettingsValidatable
{
    public int SelectedChatModelId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();

    IEnumerable<IPromptParameterValidatable> IAiSettingsValidatable.PromptParameters => PromptParameter;
}
