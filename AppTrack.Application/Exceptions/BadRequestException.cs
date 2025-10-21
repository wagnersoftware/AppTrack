

using FluentValidation.Results;

namespace AppTrack.Application.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request is invalid or cannot be processed due to client errors, such
/// as validation failures.
/// </summary>
/// <remarks>Use this exception to indicate that the client has provided input that does not meet the required
/// criteria. The exception may include detailed validation errors to help the caller identify and correct issues in the
/// request. This exception is typically used in web APIs to signal HTTP 400 Bad Request responses.</remarks>
public class BadRequestException : Exception
{
    public IDictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();
    public BadRequestException(string message) : base(message)
    {

    }

    public BadRequestException(string message, ValidationResult validationResult) : base(message)
    {
        ValidationErrors = validationResult.ToDictionary();
    }
}
