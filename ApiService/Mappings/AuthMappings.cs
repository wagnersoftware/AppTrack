using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class AuthMappings
{
    internal static AuthRequest ToAuthRequest(this LoginModel model) => new()
    {
        UserName = model.UserName,
        Password = model.Password,
    };

    internal static RegistrationRequest ToRegistrationRequest(this RegistrationModel model) => new()
    {
        UserName = model.UserName,
        Password = model.Password,
    };
}
