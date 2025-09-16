using System.Net;

namespace AppTrack.BlazorUI.Services.Base;

public class BaseHttpService
{
    protected IClient _client;

    public BaseHttpService(IClient client)
    {
        _client = client;
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
}