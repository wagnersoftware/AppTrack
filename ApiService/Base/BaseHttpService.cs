using AppTrack.Frontend.ApiService.Contracts;

namespace AppTrack.Frontend.ApiService.Base;

public class BaseHttpService
{
    protected IClient _client;
    protected readonly ITokenStorage _tokenStorage;

    public BaseHttpService(IClient client, ITokenStorage tokenStorage)
    {
        this._client = client;
        this._tokenStorage = tokenStorage;
    }

    protected Response<Guid> ConvertApiException(ApiException apiException)
    {
        if (apiException.StatusCode == 400) // bad request
        {
            return new Response<Guid> { Message = "Invalid data was submitted", ValidationErrors = apiException.Response, Success = false };
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
        if (await _tokenStorage.ContainsKeyAsync("token"))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenStorage.GetItemAsync<string>("token"));
        }
    }
}
