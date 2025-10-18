using AppTrack.Api.IntegrationTests.Seeddata.AiSetttings;
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

        // --- Seed Domain Entities ---
        await AiSettingsSeed.AddAiSettingsForUser1Async(mainDb);

        await identityDb.SaveChangesAsync();
        await mainDb.SaveChangesAsync();
    }
}