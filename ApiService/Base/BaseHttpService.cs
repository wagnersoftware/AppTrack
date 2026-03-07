using AppTrack.Frontend.ApiService.Helper;

namespace AppTrack.Frontend.ApiService.Base;

public class BaseHttpService
{
    protected IClient _client;

    public BaseHttpService(IClient client)
    {
        this._client = client;
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

    protected async Task<Response<T>> TryExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
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
