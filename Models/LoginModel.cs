using AppTrack.Frontend.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class LoginModel : ModelBase
{
    [Required]
    [MaxLength(256, ErrorMessage = "{0} must not exceed 256 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
