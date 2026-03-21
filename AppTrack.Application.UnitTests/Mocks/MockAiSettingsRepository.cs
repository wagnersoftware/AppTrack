using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockAiSettingsRepository
{
    public static Mock<IAiSettingsRepository> GetMock()
    {
        var mockRepo = new Mock<IAiSettingsRepository>();

        var aiSettings = new AiSettings
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
        };

        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.GetByIdWithPromptParameterAsync(1)).ReturnsAsync(aiSettings);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<AiSettings>())).Returns(Task.CompletedTask);

        return mockRepo;
    }
}
