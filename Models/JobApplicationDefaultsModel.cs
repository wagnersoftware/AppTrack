using AppTrack.Frontend.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class JobApplicationDefaultsModel : ModelBase
{
    [MaxLength(50, ErrorMessage = "{0} must not exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "{0} must not exceed 50 characters.")]
    public string Position { get; set; } = string.Empty;
    
    [MaxLength(50, ErrorMessage = "{0} must not exceed 50 characters.")]
    public string Location { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
