using AppTrack.Api.IntegrationTests.Seeddata.AiSetttings;
using AppTrack.Api.IntegrationTests.Seeddata.JobApplicationDefaults;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Identity.DBContext;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Api.IntegrationTests.Seeddata;

internal static class SeedTestData
{
    internal static async Task SeedDataAsync(AppTrackDatabaseContext mainDb, AppTrackIdentityDbContext identityDb)
    {
        // --- Seed Identity Users ---
         await ApplicationUserSeed.AddUserAsync(identityDb);

        // --- Ai Settings ---
        await AiSettingsSeed.AddAiSettingsForUser1Async(mainDb);
        await AiSettingsSeed.AddAiSettingsForRandomUserAsync(mainDb);

        // --- Job Application Defaults ---
        await JobApplicationDefaultsSeed.AddJobApplicationDefaultsForUser1Async(mainDb);
        await JobApplicationDefaultsSeed.AddJobApplicationDefaultsForRandomUserAsync(mainDb);
    }
}