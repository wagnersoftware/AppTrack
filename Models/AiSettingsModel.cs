using AppTrack.Frontend.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string PromptTemplate { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    [ObservableProperty]
    private List<PromptParameterModel> promptParameter = new List<PromptParameterModel>();
}
