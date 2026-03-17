using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetAiSettingsByUserIdQueryHandlerTests
{
    private readonly Mock<IAiSettingsRepository> _mockRepo;

    public GetAiSettingsByUserIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IAiSettingsRepository>();
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingAiSettings_WhenAiSettingsExistForUser()
    {
        const string userId = "user-1";
        var existingSettings = new DomainAiSettings
        {
            Id = 1,
            UserId = userId,
            SelectedChatModelId = 3,
        };

        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(existingSettings);

        var query = new GetAiSettingsByUserIdQuery { UserId = userId };
        var handler = new GetAiSettingsByUserIdQueryHandler(_mockRepo.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<AiSettingsDto>();
        result.UserId.ShouldBe(userId);
        result.Id.ShouldBe(1);
        result.SelectedChatModelId.ShouldBe(3);
        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<DomainAiSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndReturnNewAiSettings_WhenNoAiSettingsExistForUser()
    {
        const string userId = "new-user";

        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync((DomainAiSettings?)null);

        _mockRepo
            .Setup(r => r.CreateAsync(It.IsAny<DomainAiSettings>()))
            .Returns(Task.CompletedTask);

        var query = new GetAiSettingsByUserIdQuery { UserId = userId };
        var handler = new GetAiSettingsByUserIdQueryHandler(_mockRepo.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<AiSettingsDto>();
        result.UserId.ShouldBe(userId);
        _mockRepo.Verify(r => r.CreateAsync(It.Is<DomainAiSettings>(s => s.UserId == userId)), Times.Once);
    }
}
