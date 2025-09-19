using AppTrack.BlazorUI.Models;

namespace AppTrack.BlazorUI.Contracts
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(LoginVM loginVM);

        Task<bool> RegisterAsync(RegisterVM registerVM);

        Task Logout();
    }
}
