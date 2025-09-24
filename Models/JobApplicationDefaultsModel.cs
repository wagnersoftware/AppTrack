using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class JobApplicationDefaultsModel: ModelBase
{
    [MaxLength(50, ErrorMessage = "Maximum length for name is 50 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Maximum length for name is 30 characters")]
    public string Position { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
