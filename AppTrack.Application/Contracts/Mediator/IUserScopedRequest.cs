namespace AppTrack.Application.Contracts.Mediator;

/// <summary>
/// Marker interface for commands and queries that require the authenticated user's ID.
/// Implement this interface on any <see cref="IRequest{TResponse}"/> that must be scoped
/// to the calling user. The <see cref="UserId"/> property is never supplied by API callers
/// or controllers; it is injected by the <c>UserScopedRequestBehavior</c> in the Infrastructure
/// layer before the handler executes, ensuring that user identity always originates from the
/// verified JWT claims and cannot be spoofed via request parameters.
/// </summary>
public interface IUserScopedRequest
{
    /// <summary>
    /// Gets or sets the ID of the authenticated user that owns this request.
    /// The setter must only be called by the pipeline behavior; controllers and
    /// other callers must not set this property.
    /// </summary>
    string UserId { get; set; }
}
