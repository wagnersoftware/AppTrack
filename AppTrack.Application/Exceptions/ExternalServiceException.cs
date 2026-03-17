using System.Net;

namespace AppTrack.Application.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a call to an external service fails.
/// </summary>
/// <remarks>Use this exception to surface errors originating from third-party APIs (e.g. OpenAI, SendGrid)
/// in a structured way. The optional <see cref="UpstreamStatusCode"/> carries the HTTP status code returned
/// by the upstream service, allowing the API middleware to produce an appropriate response to the caller.</remarks>
public class ExternalServiceException : Exception
{
    /// <summary>Gets the HTTP status code returned by the upstream service, if available.</summary>
    public HttpStatusCode? UpstreamStatusCode { get; }

    public ExternalServiceException(string message)
        : base(message)
    {
    }

    public ExternalServiceException(string message, HttpStatusCode? upstreamStatusCode)
        : base(message)
    {
        UpstreamStatusCode = upstreamStatusCode;
    }

    public ExternalServiceException(string message, HttpStatusCode? upstreamStatusCode, Exception innerException)
        : base(message, innerException)
    {
        UpstreamStatusCode = upstreamStatusCode;
    }
}
