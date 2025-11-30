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
            PromptParameter = new List<PromptParameter>()
            {
                PromptParameter.Create("key1", "value1"),
                PromptParameter.Create("key2", "value2"),
            }
        };

        var promptParameter1 = PromptParameter.Create("name", "Daniel");
        var promptParameter2 = PromptParameter.Create("rate", "150");

        aiSettings.PromptParameter = new List<PromptParameter> { promptParameter1, promptParameter2 };

        await db.AiSettings.AddAsync(aiSettings);
        await db.SaveChangesAsync();

        return aiSettings.Id;
    }

    /// <summary>
    /// Asynchronously creates and adds predefined chat model entries to the application's database using the provided
    /// service provider.
    /// </summary>
    /// <remarks>This method is intended for internal initialization of chat models and should be called
    /// during application setup or data seeding. The operation will add models only if they do not already exist in the
    /// database.</remarks>
    /// <param name="services">The service provider used to resolve database context and related dependencies required for model creation.</param>
    /// <returns>A task that represents the asynchronous operation of creating and saving chat models to the database.</returns>
    internal static async Task CreateChatModelsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var chatmodels = new List<ChatModel>()
        {
            new ChatModel
            {
                Name = "ChatGPT 3.5",
                ApiModelName = "gpt-3.5-turbo",
                Description = "Fast model, suitable for short text snippets and suggestions",
                IsActive = true
            },
            new ChatModel
            {
                Name = "ChatGPT 4",
                ApiModelName = "gpt-4",
                Description = "High-precision model, ideal for complex cover letters and refined writing",
                IsActive = true
            },
            new ChatModel
            {
                Name = "ChatGPT 4 Mini",
                ApiModelName = "gpt-4o-mini",
                Description = "Lightweight model for quick suggestions or interactive text generation",
                IsActive = false
            }
        };

        foreach (var chatmodel in chatmodels)
        {
            await db.ChatModels.AddAsync(chatmodel);
        }

        await db.SaveChangesAsync();

    }
}
