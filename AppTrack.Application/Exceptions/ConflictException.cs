namespace AppTrack.Application.Exceptions;

/// <summary>
/// Represents an error that occurs when an operation cannot be completed due to a conflict with the current state of
/// the resource.
/// </summary>
/// <remarks>This exception is typically thrown when attempting to perform an action that would violate a
/// uniqueness constraint or when a resource is in a state that prevents the requested operation from succeeding. Use
/// this exception to indicate that the failure is due to a logical conflict, such as attempting to create a duplicate
/// entry or update a resource that has changed since it was last retrieved.</remarks>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
