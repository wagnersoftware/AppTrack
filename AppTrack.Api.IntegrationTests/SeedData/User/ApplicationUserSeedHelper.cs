namespace AppTrack.Api.IntegrationTests.Seeddata.User;

internal static class ApplicationUserSeedHelper
{
    /// <summary>
    /// Creates a new test user identifier for use in integration tests.
    /// </summary>
    /// <remarks>Returns a stable or randomly generated user identifier suitable for seeding
    /// test data. No Identity database record is created; the ID is used purely as a
    /// foreign-key value in the application database.</remarks>
    /// <param name="services">Unused. Retained for call-site compatibility.</param>
    /// <param name="userName">Unused. Retained for call-site compatibility.</param>
    /// <param name="userId">An optional fixed user identifier. A new GUID is generated when null.</param>
    /// <returns>A string containing the user identifier.</returns>
    internal static Task<string> CreateTestUserAsync(
        IServiceProvider services,
        string? userName = null,
        string? userId = null)
    {
        return Task.FromResult(userId ?? Guid.NewGuid().ToString());
    }
}
