using AppTrack.Frontend.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class RegistrationModel : ModelBase
{
    
    [Required(ErrorMessage = "The UserName field is required.")]
    [UserNameValidation]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [PasswordValidation]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
