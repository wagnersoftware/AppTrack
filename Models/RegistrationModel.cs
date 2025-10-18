using AppTrack.Frontend.Models.Base;
using AppTrack.Frontend.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class RegistrationModel : ModelBase
{

    [UserNameValidation]
    public string UserName { get; set; } = string.Empty;

    [PasswordValidation]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
