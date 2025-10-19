using AppTrack.Frontend.Models.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase
{
    public string UserId { get; set; } = string.Empty;

    [RegularExpression("^sk-[A-Za-z0-9]{20,}$", ErrorMessage = "ApiKey must be a valid OpenAI API key.")]
    public string ApiKey { get; set; } = string.Empty;

    public string PromptTemplate { get; set; } = string.Empty;

    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
}
