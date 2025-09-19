using Blazored.LocalStorage;
using System.Net;

namespace AppTrack.BlazorUI.Services.Base;

public class BaseHttpService
{
    protected IClient _client;
    protected readonly ILocalStorageService _localStorageService;

    public BaseHttpService(IClient client, ILocalStorageService localStorageService)
    {
        this._client = client;
        this._localStorageService = localStorageService;
    }

    protected Response<Guid> ConvertApiException(ApiException apiException)
    {
        if(apiException.StatusCode == 400) // bad request
        {
            return new Response<Guid> {  Message = "Invalid data was submitted", ValidationErrors = apiException.Response, Success = false };
        }
        else if (apiException.StatusCode == 404) // not found
        {
            return new Response<Guid> { Message = "The record was not found", Success = false };
        }
        else 
        {
            return new Response<Guid> { Message = "Something went wrong, please try again", Success = false };
        }
    }

    protected async Task AddBearerTokenAsync()
    {
        if(await _localStorageService.ContainKeyAsync("token"))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _localStorageService.GetItemAsync<string>("token"));
        }
    }
}