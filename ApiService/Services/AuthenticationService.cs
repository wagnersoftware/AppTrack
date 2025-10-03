using AppTrack.Frontend.ApiService.ApiAuthenticationProvider;
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppTrack.Frontend.ApiService.Services;

public class AuthenticationService : BaseHttpService, IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    public AuthenticationService(IClient client,
                                 ITokenStorage localStorage,
                                 IMapper mapper,
                                 AuthenticationStateProvider authenticationStateProvider) : base(client, localStorage)
    {
        this._mapper = mapper;
        this._authenticationStateProvider = authenticationStateProvider;
    }

    public Task<Response<bool>> AuthenticateAsync(LoginModel loginModel) => 
           TryExecuteAsync(async () =>
           {
               var authenticationRequest = _mapper.Map<AuthRequest>(loginModel);
               var authenticationResponse = await _client.LoginAsync(authenticationRequest);
               if (authenticationResponse.Token != string.Empty)
               {
                   await _tokenStorage.SetItemAsync("token", authenticationResponse.Token);
                   await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedIn();
                   return true;
               }
               return false;
           });

    public async Task Logout()
    {
        await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedOut();
    }

    public Task<Response<bool>> RegisterAsync(RegistrationModel registerModel) =>
       TryExecuteAsync(async () =>
       {
           var registrationRequest = _mapper.Map<RegistrationRequest>(registerModel);
           var response = await _client.RegisterAsync(registrationRequest);

           if (!string.IsNullOrEmpty(response.UserId))
           {
               return true;
           }
           return false;
       });
}
