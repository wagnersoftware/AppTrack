using AppTrack.Application.Contracts;

namespace AppTrack.Functions.Identity;

/// <summary>
/// No-op implementation of <see cref="IUserContext"/> used in the Azure Functions host,
/// where there is no HTTP context. This implementation is safe because all commands
/// dispatched from timer-triggered functions must not implement
/// <see cref="AppTrack.Application.Contracts.Mediator.IUserScopedRequest"/>; the mediator
/// checks <see cref="IsAuthenticated"/> before attempting to resolve a user ID and will
/// throw if a user-scoped request is dispatched without an authenticated user.
/// </summary>
internal sealed class NullUserContext : IUserContext
{
    /// <inheritdoc/>
    public bool IsAuthenticated => false;

    /// <inheritdoc/>
    public string GetCurrentUserId() =>
        throw new InvalidOperationException(
            "GetCurrentUserId is not supported in the Azure Functions host. " +
            "Timer-triggered functions must only dispatch commands that do not implement IUserScopedRequest.");
}
