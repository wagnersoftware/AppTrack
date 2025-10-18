using System.ComponentModel.DataAnnotations;

namespace AppTrack.Application.Models.Identity;

public class RegistrationRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
