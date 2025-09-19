using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Formats.Asn1;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AppTrack.BlazorUI.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public ApiAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            this._localStorageService = localStorageService;
            this._jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var isTokenPresent = await _localStorageService.ContainKeyAsync("token");

            if (isTokenPresent == false)
            {
                return new AuthenticationState(user);
            }

            var savedToken = await _localStorageService.GetItemAsync<string>("token");
            var tokenContent = _jwtSecurityTokenHandler.ReadJwtToken(savedToken);

            if (tokenContent.ValidTo < DateTime.Now)
            {
                await _localStorageService.RemoveItemAsync("token");
                return new AuthenticationState(user);
            }

            var claims = await GetClaimsAsync();
            user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            return new AuthenticationState(user);
        }

        public async Task LoggedOut()
        {
            var claims = await GetClaimsAsync();
            var nobody = new ClaimsPrincipal(new ClaimsIdentity());
            var authstate = Task.FromResult(new AuthenticationState(nobody));
            NotifyAuthenticationStateChanged(authstate);
        }

        public async Task LoggedIn()
        {
            var claims = await GetClaimsAsync();
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authstate = Task.FromResult(new AuthenticationState(user)); 
            NotifyAuthenticationStateChanged(authstate);
        }

        private async Task<List<Claim>> GetClaimsAsync()
        {
            var savedToken = await _localStorageService.GetItemAsync<string>("token");
            var tokenContent = _jwtSecurityTokenHandler.ReadJwtToken(savedToken);
            var claims = tokenContent.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject));
            return claims;
        }
    }
}
