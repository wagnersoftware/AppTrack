using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class AiSettingsModel : ModelBase
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string MySkills { get; set; } = string.Empty;

    [Required]
    public string Prompt { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
