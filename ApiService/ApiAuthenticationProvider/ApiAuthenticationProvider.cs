using AppTrack.Frontend.ApiService.Contracts;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AppTrack.Frontend.ApiService.ApiAuthenticationProvider;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorage _localStorageService;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    public ApiAuthenticationStateProvider(ITokenStorage localStorageService)
    {
        this._localStorageService = localStorageService;
        this._jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var isTokenPresent = await _localStorageService.ContainsKeyAsync("token");

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
        await _localStorageService.RemoveItemAsync("token");
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
