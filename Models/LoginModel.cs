using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Frontend.Models;

public class LoginModel : ModelBase, IUserCredentialsValidatable
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
