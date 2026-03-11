using AppTrack.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace AppTrack.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core implementation of <see cref="IUserContext"/>.
/// Reads the authenticated user's object ID from the current HTTP context claims.
/// </summary>
/// <remarks>
/// Claim resolution order:
/// 1. <c>"oid"</c> — Azure AD / CIAM object identifier (used in production with Microsoft.Identity.Web).
/// 2. <c>"sub"</c> — Standard OIDC subject identifier (used by the test authentication handler in integration tests).
/// Throws <see cref="InvalidOperationException"/> if neither claim is present or if there is no active HTTP context.
/// </remarks>
internal sealed class HttpContextUserContext : IUserContext
{
    /// <summary>Azure AD / CIAM object identifier claim name.</summary>
    private const string OidClaimType = "oid";

    /// <summary>Standard OIDC subject identifier claim name.</summary>
    private const string SubClaimType = "sub";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <inheritdoc/>
    public string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException(
                "GetCurrentUserId was called outside of an active HTTP request context.");

        // Prefer the Azure AD object identifier ("oid"); fall back to the standard subject claim ("sub").
        var userId = user.FindFirst(OidClaimType)?.Value
            ?? user.FindFirst(SubClaimType)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException(
                $"The authenticated user's ID could not be resolved from JWT claims. " +
                $"Expected claim '{OidClaimType}' or '{SubClaimType}' to be present.");
        }

        return userId;
    }
}
