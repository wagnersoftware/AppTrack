using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Models.Identity;

public class AuthRequest : IUserCredentialsValidatable
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
