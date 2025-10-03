using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAuthenticationService
{
    Task<bool> AuthenticateAsync(LoginModel loginModel);

    Task<bool> RegisterAsync(RegistrationModel registerModel);

    Task Logout();
}
