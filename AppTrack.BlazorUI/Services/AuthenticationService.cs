using AppTrack.BlazorUI.Contracts;
using AppTrack.BlazorUI.Models;
using AppTrack.BlazorUI.Providers;
using AppTrack.BlazorUI.Services.Base;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;

namespace AppTrack.BlazorUI.Services;

public class AuthenticationService : BaseHttpService, IAuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(IClient client, ILocalStorageService localStorageService, AuthenticationStateProvider authenticationStateProvider) 
        : base(client, localStorageService)
    {
        this._authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> AuthenticateAsync(LoginVM loginVM)
    {
        try
        {
            var authRequest = new AuthRequest()
            {
                Email = loginVM.Email,
                Password = loginVM.Password,
            };

            var authResponse = await _client.LoginAsync(authRequest);

            if (authResponse.Token != string.Empty)
            {
                await _localStorageService.SetItemAsync("token", authResponse.Token);
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

    public async Task<bool> RegisterAsync(RegisterVM registerVM)
    {
        var registrationRequest = new RegistrationRequest()
        {
            Email = registerVM.Email,
            FirstName = registerVM.FirstName,
            LastName = registerVM.LastName,
            UserName = registerVM.UserName,
            Password = registerVM.Password
        };

        var registrationResponse = await _client.RegisterAsync(registrationRequest);

        if (!string.IsNullOrEmpty(registrationResponse.UserId))
        {
            return true;
        }

        return false;
    }
}
