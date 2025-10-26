using AppTrack.Frontend.Models.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase
{
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "{0} must not exceed 200 characters.")]
    public string ApiKey { get; set; } = string.Empty;

    public string PromptTemplate { get; set; } = string.Empty;

    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
}
