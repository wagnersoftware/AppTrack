namespace AppTrack.Frontend.ApiService.Base;

public class Response<T>
{
    /// <summary>
    /// A generic error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Contains detailed error information from the backend (e.g. field-level validation messages).
    /// </summary>
    public string ErrorDetails { get; set; } = string.Empty;

    /// <summary>
    /// Returns ErrorDetails if available, otherwise falls back to ErrorMessage.
    /// Use this as the single source for displaying an error to the user.
    /// </summary>
    public string DisplayMessage => !string.IsNullOrEmpty(ErrorDetails) ? ErrorDetails : ErrorMessage;

    /// <summary>
    /// False in error case, otherwise true,
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data, not null in success case.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// HTTP status code of the response. 0 if unknown (e.g. network error).
    /// </summary>
    public int StatusCode { get; set; }
}