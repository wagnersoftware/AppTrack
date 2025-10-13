using AppTrack.Frontend.Models.Base;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string PromptTemplate { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;


    public ObservableCollection<PromptParameterModel> PromptParameter { get; set; } = new ObservableCollection<PromptParameterModel>();
}
