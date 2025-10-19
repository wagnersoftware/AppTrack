using AppTrack.Api.IntegrationTests.SeedData.User;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Api.IntegrationTests.SeedData.AiSetttings;

internal static class AiSettingsSeed
{
    internal static int AiSettings1Id { get; private set; }
    internal static int AiSettings2Id { get; private set; }

    internal static async Task AddAiSettingsForUser1Async(AppTrackDatabaseContext dbContext)
    {
        var aiSettingsUser1 = new AiSettings
        {
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "1234abc",
            PromptTemplate = "Hello, my name is {name} and my rate is {rate}.",
        };

        var promptParameter1 = PromptParameter.Create("name", "Daniel");
        var promptParameter2 = PromptParameter.Create("rate", "150");

        aiSettingsUser1.PromptParameter = new List<PromptParameter> { promptParameter1, promptParameter2 };

        await dbContext.AiSettings.AddAsync(aiSettingsUser1);
        await dbContext.SaveChangesAsync();
        AiSettings1Id = aiSettingsUser1.Id;
    }

    internal static async Task AddAiSettingsForRandomUserAsync(AppTrackDatabaseContext dbContext)
    {
        var aiSettingsRandomUser = new AiSettings
        {
            UserId = Guid.NewGuid().ToString(),
            ApiKey = "1234abc",
            PromptTemplate = "Hello, my name is {name} and my rate is {rate}.",
        };

        var promptParameter1 = PromptParameter.Create("name", "Daniel");
        var promptParameter2 = PromptParameter.Create("rate", "150");

        aiSettingsRandomUser.PromptParameter = new List<PromptParameter> { promptParameter1, promptParameter2 };

        await dbContext.AiSettings.AddAsync(aiSettingsRandomUser);
        await dbContext.SaveChangesAsync();
        AiSettings2Id = aiSettingsRandomUser.Id;
    }
}
