using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Frontend.Models;

public class RegistrationModel : ModelBase, IRegistrationValidatable
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
