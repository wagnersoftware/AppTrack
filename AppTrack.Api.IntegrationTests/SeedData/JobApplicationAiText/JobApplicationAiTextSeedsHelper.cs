using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.JobApplicationAiText;

internal static class JobApplicationAiTextSeedsHelper
{
    public static async Task<int> CreateAiTextForJobApplicationAsync(
        IServiceProvider services,
        int jobApplicationId,
        string promptKey = "cover-letter",
        string generatedText = "Test generated text.")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var aiText = new Domain.JobApplicationAiText
        {
            JobApplicationId = jobApplicationId,
            PromptKey = promptKey,
            GeneratedText = generatedText,
            GeneratedAt = DateTime.UtcNow,
        };

        await db.JobApplicationAiTexts.AddAsync(aiText);
        await db.SaveChangesAsync();

        return aiText.Id;
    }

    /// <summary>
    /// Creates complete seed data (chat model + AI settings) needed to call
    /// POST /api/ai-settings/generate-application-text without validation errors.
    /// Returns the created chat model ID and AI settings ID.
    /// </summary>
    public static async Task<(int chatModelId, int aiSettingsId)> CreateAiSettingsWithChatModelAsync(
        IServiceProvider services,
        string userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var chatModel = new Domain.ChatModel
        {
            Name = "Test Model",
            ApiModelName = "test-model",
            Description = "Stub model for integration tests",
            IsActive = true,
        };
        await db.ChatModels.AddAsync(chatModel);
        await db.SaveChangesAsync();

        var aiSettings = new Domain.AiSettings
        {
            UserId = userId,
            SelectedChatModelId = chatModel.Id,
        };
        await db.AiSettings.AddAsync(aiSettings);
        await db.SaveChangesAsync();

        return (chatModel.Id, aiSettings.Id);
    }
}
