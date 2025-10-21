namespace AppTrack.Application.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a requested entity cannot be found.
/// </summary>
/// <remarks>This exception is typically used to indicate that an operation failed because a specific resource,
/// record, or object identified by a key does not exist. It is commonly thrown in data access scenarios when a lookup
/// by name and key yields no result.</remarks>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) : base($"{name} {key} not found")
    {

    }
}
