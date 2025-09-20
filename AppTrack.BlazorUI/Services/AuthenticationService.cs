using AppTrack.BlazorUI.Providers;
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppTrack.BlazorUI.Services;

public class AuthenticationService : BaseHttpService, IAuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(IClient client, ITokenStorage tokenStorage, AuthenticationStateProvider authenticationStateProvider) 
        : base(client, tokenStorage)
    {
        this._authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> AuthenticateAsync(LoginModel loginModel)
    {
        try
        {
            var authRequest = new AuthRequest()
            {
                Email = loginModel.Email,
                Password = loginModel.Password,
            };

            var authResponse = await _client.LoginAsync(authRequest);

            if (authResponse.Token != string.Empty)
            {
                await _tokenStorage.SetItemAsync("token", authResponse.Token);
                await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedIn();
                return true;
            }

            return false;

        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task Logout()
    {
        await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedOut();
    }

    public async Task<bool> RegisterAsync(RegisterModel registerModel)
    {
        var registrationRequest = new RegistrationRequest()
        {
            Email = registerModel.Email,
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName,
            UserName = registerModel.UserName,
            Password = registerModel.Password
        };

        var registrationResponse = await _client.RegisterAsync(registrationRequest);

        if (!string.IsNullOrEmpty(registrationResponse.UserId))
        {
            return true;
        }

        return false;
    }
}
