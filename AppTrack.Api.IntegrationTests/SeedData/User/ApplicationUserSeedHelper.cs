using AppTrack.Identity.DBContext;
using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.Seeddata.User;

internal static class ApplicationUserSeedHelper
{
    /// <summary>
    /// Asynchronously creates a new test user in the specified identity database and returns the user's unique
    /// identifier.
    /// </summary>
    /// <remarks>The created test user will have a confirmed email and a default password of "Test1234!". This
    /// method is intended for testing scenarios and should not be used to create production users.</remarks>
    /// <param name="identityDb">The identity database context in which the test user will be created. Must not be null.</param>
    /// <param name="userName">The user name to assign to the test user. If null, a unique user name will be generated automatically.</param>
    /// <returns>A string containing the unique identifier of the newly created test user.</returns>
    internal static async Task<string> CreateTestUserAsync(IServiceProvider services, string? userName = null)
    {
        using var scope = services.CreateScope();
        var identityDb = scope.ServiceProvider.GetRequiredService<AppTrackIdentityDbContext>();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName ?? $"testuser_{Guid.NewGuid():N}",
            EmailConfirmed = true
        };

        var hasher = new PasswordHasher<IdentityUser>();
        user.PasswordHash = hasher.HashPassword(user, "Test1234!");

        await identityDb.Users.AddAsync(user);
        await identityDb.SaveChangesAsync();

        return user.Id;
    }
}
