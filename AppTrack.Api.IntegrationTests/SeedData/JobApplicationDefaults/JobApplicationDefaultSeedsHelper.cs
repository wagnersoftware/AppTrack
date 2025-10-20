using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.Seeddata.JobApplicationDefaults;

internal static class JobApplicationDefaultsSeedHelper
{
    /// <summary>
    /// Creates a new set of default job application settings for the specified user and saves them to the database
    /// asynchronously.
    /// </summary>
    /// <remarks>This method creates a new database scope and adds default job application settings for the
    /// specified user. The returned identifier can be used to reference the created defaults in subsequent
    /// operations.</remarks>
    /// <param name="services">The service provider used to resolve required dependencies, including the database context.</param>
    /// <param name="userId">The unique identifier of the user for whom the default job application settings will be created. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the newly created
    /// default job application settings.</returns>
    public static async Task<int> CreateDefaultsForUserAsync(IServiceProvider services, string userId)
    {
        using var  scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var defaults = new Domain.JobApplicationDefaults
        {
            UserId = userId,
            Name = "Default Name",
            Position = "Default Position",
            Location = "Default Location"
        };

        await db.JobApplicationDefaults.AddAsync(defaults);
        await db.SaveChangesAsync();

        return defaults.Id;
    }
}
