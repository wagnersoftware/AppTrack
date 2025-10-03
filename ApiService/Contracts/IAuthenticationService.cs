using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAuthenticationService
{
    Task<Response<bool>> AuthenticateAsync(LoginModel loginModel);

    Task<Response<bool>> RegisterAsync(RegistrationModel registerModel);

    Task Logout();
}
