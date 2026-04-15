using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockAiSettingsRepository
{
    public const string ExistingUserId = "user1";

    public static Mock<IAiSettingsRepository> GetMock()
    {
        var mockRepo = new Mock<IAiSettingsRepository>();

        var aiSettings = new AiSettings
        {
            Id = 1,
            UserId = ExistingUserId,
            SelectedChatModelId = 1,
        };

        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.GetByIdWithPromptsAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<AiSettings>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<AiSettings>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.GetByUserIdWithPromptParameterAsync(ExistingUserId)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.GetByUserIdWithPromptParameterAsync(It.Is<string>(id => id != ExistingUserId))).ReturnsAsync((AiSettings?)null);

        return mockRepo;
    }
}
