using System.ComponentModel.DataAnnotations;

namespace AppTrack.Frontend.Models;

public class LoginModel : ModelBase
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
