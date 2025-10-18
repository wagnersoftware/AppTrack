using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Api.IntegrationTests.Seeddata.AiSetttings;

internal static class AiSettingsSeed
{
    internal static async Task AddAiSettingsForUser1Async(AppTrackDatabaseContext dbContext)
    {
        var aiSettingsUser1 = new AiSettings
        {
            UserId = ApplicationUserSeed.User1Id,
            ApiKey = "1234abc",
            PromptTemplate = "Hello, my name is {name} and my rate is {rate}.",
            CreationDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
        };

        var promptParameter1 = PromptParameter.Create("name", "Daniel");
        var promptParameter2 = PromptParameter.Create("rate", "150");

        aiSettingsUser1.PromptParameter = new List<PromptParameter> { promptParameter1, promptParameter2 };

        await dbContext.AiSettings.AddAsync(aiSettingsUser1);
    }
}
