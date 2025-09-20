using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAuthenticationService
{
    Task<bool> AuthenticateAsync(LoginModel loginVM);

    Task<bool> RegisterAsync(RegisterModel registerVM);

    Task Logout();
}
