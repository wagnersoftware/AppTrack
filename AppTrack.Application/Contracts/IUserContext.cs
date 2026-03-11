namespace AppTrack.Application.Contracts;

/// <summary>
/// Provides the ID of the currently authenticated user.
/// Implemented in the Infrastructure layer using <c>IHttpContextAccessor</c>,
/// keeping the Application layer free of any ASP.NET Core dependency.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Returns <c>true</c> when an authenticated user is present in the current HTTP context.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns the authenticated user's object ID from JWT claims.
    /// Throws <see cref="InvalidOperationException"/> if called outside an
    /// authenticated HTTP context or when the claim is absent.
    /// </summary>
    string GetCurrentUserId();
}
