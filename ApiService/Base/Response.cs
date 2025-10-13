namespace AppTrack.Frontend.ApiService.Base;

public class Response<T>
{
    /// <summary>
    /// A generic error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Contains specific validation errors.
    /// </summary>
    public string ValidationErrors { get; set; } = string.Empty;

    /// <summary>
    /// False in error case, otherwise true,
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data, not null in success case.
    /// </summary>
    public T? Data { get; set; }
}