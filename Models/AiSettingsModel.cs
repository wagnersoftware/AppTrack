using AppTrack.Frontend.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public partial class AiSettingsModel : ModelBase
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string MySkills { get; set; } = string.Empty;

    [Required]
    public string Prompt { get; set; } = string.Empty;

    [ObservableProperty]
    private List<KeyValueItemModel> promptParameter = new List<KeyValueItemModel>();
}
