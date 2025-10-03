using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Helper;

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

    private Response<T> ConvertApiException<T>(ApiException apiException)
    {
        if (apiException.StatusCode == 400) // bad request
        {
            return new Response<T> { Message = "Invalid data was submitted", ValidationErrors = ApiErrorHelper.ExtractErrors(apiException.Response), Success = false };
        }
        else if (apiException.StatusCode == 404) // not found
        {
            return new Response<T> { Message = "The record was not found", Success = false };
        }
        else
        {
            return new Response<T> { Message = "Something went wrong, please try again", Success = false };
        }
    }

    private async Task AddBearerTokenAsync()
    {
        if (await _tokenStorage.ContainsKeyAsync("token"))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenStorage.GetItemAsync<string>("token"));
        }
    }

    protected async Task<Response<T>> TryExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            await AddBearerTokenAsync();
            var result = await action();
            return new Response<T> { Success = true, Data = result };
        }
        catch (ApiException e)
        {
            return ConvertApiException<T>(e);
        }
    }

    protected async Task<Response<T>> TryExecuteAsync<T>(Func<Task> action)
    {
        try
        {
            await AddBearerTokenAsync();
            await action();
            return new Response<T> { Success = true };
        }
        catch (ApiException e)
        {
            return ConvertApiException<T>(e);
        }
    }
}
