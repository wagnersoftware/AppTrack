using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class RegistrationModel : ModelBase
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
