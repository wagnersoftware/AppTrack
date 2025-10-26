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

    private Response<T> ConvertApiException<T>(ApiException apiEx)
    {
        string validationErrors = string.Empty;

        if (apiEx is ApiException<CustomProblemDetails> typedException && typedException.Result != null)
        {
            validationErrors = ApiErrorHelper.ExtractErrors(typedException.Result);
        }
        else if (!string.IsNullOrEmpty(apiEx.Response))
        {
            validationErrors = ApiErrorHelper.ExtractErrors(apiEx.Response);
        }

        return apiEx.StatusCode switch
        {
            400 => new Response<T> { ErrorMessage = "Invalid data was submitted", ValidationErrors = validationErrors, Success = false },
            404 => new Response<T> { ErrorMessage = "The record was not found", ValidationErrors = validationErrors, Success = false },
            _ => new Response<T> { ErrorMessage = "Something went wrong, please try again", ValidationErrors = validationErrors, Success = false },
        };
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
        catch (OperationCanceledException)
        {
            return new Response<T> { Success = false, ErrorMessage = "Operation canceled." };
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
        catch (OperationCanceledException)
        {
            return new Response<T> { Success = false, ErrorMessage = "Operation canceled." };
        }
        catch (ApiException e)
        {
            return ConvertApiException<T>(e);
        }
    }
}
