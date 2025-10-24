using AppTrack.Api.IntegrationTests.Seeddata.JobApplicationDefaults;
using AppTrack.Api.IntegrationTests.SeedData.AiSettings;
using AppTrack.Api.IntegrationTests.SeedData.JobApplication;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.Seeddata;

internal static class SeedHelper
{

    internal static async Task<(string userId, int defaultsId)> CreateUserWithJobApplicationAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userId = await User.ApplicationUserSeedHelper.CreateTestUserAsync(services);
        var jobApplicationId = await JobApplicationSeedsHelper.CreateJobApplicationForUserAsync(services, userId);

        return (userId, jobApplicationId);
    }

    /// <summary>
    /// Asynchronously creates a test user and associated job application defaults, returning their identifiers.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies required for user and job defaults creation. Must not be null.</param>
    /// <returns>A tuple containing the user identifier and the job application defaults identifier for the newly created test
    /// user.</returns>
    internal static async Task<(string userId, int defaultsId)> CreateUserWithJobDefaultsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userId = await User.ApplicationUserSeedHelper.CreateTestUserAsync(services);
        var defaultsId = await JobApplicationDefaultsSeedHelper.CreateDefaultsForUserAsync(services, userId);

        return (userId, defaultsId);
    }

    /// <summary>
    /// Asynchronously creates a test user and associated AI settings, returning their identifiers.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies required for user and AI settings creation. Cannot be null.</param>
    /// <returns>A tuple containing the user ID and the AI settings ID for the newly created test user.</returns>
    internal static async Task<(string userId, int aiSettingsId)> CreateUserWithAiSettingsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userId = await User.ApplicationUserSeedHelper.CreateTestUserAsync(services);
        var aiSettingsId = await AiSettingsSeedsHelper.CreateAiSettingsForUserAsync(services, userId);
        return (userId, aiSettingsId);
    }
}
