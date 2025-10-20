using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.AiSettings;

internal static class AiSettingsSeedsHelper
{
    /// <summary>
    /// Asynchronously creates a new AI settings record for the specified user and returns its unique identifier.
    /// </summary>
    /// <param name="services">The service provider used to resolve required services, such as the database context. Must not be null.</param>
    /// <param name="userId">The unique identifier of the user for whom the AI settings will be created. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    /// created AI settings record.</returns>
    public static async Task<int> CreateAiSettingsForUserAsync(IServiceProvider services, string userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var aiSettings = new Domain.AiSettings
        {
            UserId = userId,
            ApiKey = "1234abc",
            PromptTemplate = "Hello, my name is {name} and my rate is {rate}.",
        };

        var promptParameter1 = PromptParameter.Create("name", "Daniel");
        var promptParameter2 = PromptParameter.Create("rate", "150");

        aiSettings.PromptParameter = new List<PromptParameter> { promptParameter1, promptParameter2 };

        await db.AiSettings.AddAsync(aiSettings);
        await db.SaveChangesAsync();

        return aiSettings.Id;
    }
}
