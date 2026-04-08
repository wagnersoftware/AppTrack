using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetAiSettingsByUserIdQueryHandlerTests
{
    private readonly Mock<IAiSettingsRepository> _mockRepo = new();
    private readonly Mock<IDefaultPromptRepository> _mockDefaultPromptRepo = new();

    public GetAiSettingsByUserIdQueryHandlerTests()
    {
        // Default: return empty list so existing tests are unaffected
        _mockDefaultPromptRepo
            .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DefaultPrompt>());
    }

    private GetAiSettingsByUserIdQueryHandler CreateHandler() =>
        new(_mockRepo.Object, _mockDefaultPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnExistingAiSettings_WhenAiSettingsExistForUser()
    {
        const string userId = "user-1";
        var existingSettings = new DomainAiSettings { Id = 1, UserId = userId, SelectedChatModelId = 3 };

        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(existingSettings);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

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

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        _mockRepo.Verify(r => r.CreateAsync(It.Is<DomainAiSettings>(s => s.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPopulateDefaultPrompts_InReturnedDto()
    {
        const string userId = "user-1";
        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId });

        var defaults = new List<DefaultPrompt>
        {
            DefaultPrompt.Create("Default_Anschreiben", "Template A", "de"),
            DefaultPrompt.Create("Default_Vorstellung", "Template B", "de"),
        };
        _mockDefaultPromptRepo
            .Setup(r => r.GetByLanguageAsync(It.IsAny<string>()))
            .ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.DefaultPrompts.ShouldNotBeNull();
        result.DefaultPrompts.Count.ShouldBe(2);
        result.DefaultPrompts[0].Name.ShouldBe("Default_Anschreiben");
        result.DefaultPrompts[1].Name.ShouldBe("Default_Vorstellung");
    }

    [Fact]
    public async Task Handle_ShouldRequestEnglishDefaultPrompts_WhenLanguageIsEnglish()
    {
        const string userId = "user-1";
        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId, Language = ApplicationLanguage.English });
        _mockDefaultPromptRepo
            .Setup(r => r.GetByLanguageAsync("en"))
            .ReturnsAsync(new List<DefaultPrompt>());

        await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        _mockDefaultPromptRepo.Verify(r => r.GetByLanguageAsync("en"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRequestGermanDefaultPrompts_WhenLanguageIsGerman()
    {
        const string userId = "user-1";
        _mockRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(userId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId, Language = ApplicationLanguage.German });
        _mockDefaultPromptRepo
            .Setup(r => r.GetByLanguageAsync("de"))
            .ReturnsAsync(new List<DefaultPrompt>());

        await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        _mockDefaultPromptRepo.Verify(r => r.GetByLanguageAsync("de"), Times.Once);
    }
}
