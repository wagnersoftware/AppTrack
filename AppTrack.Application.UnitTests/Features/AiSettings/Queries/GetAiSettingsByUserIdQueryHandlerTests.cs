using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetAiSettingsByUserIdQueryHandlerTests
{
    private readonly Mock<IAiSettingsRepository> _mockRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();
    private readonly Mock<IValidator<GetAiSettingsByUserIdQuery>> _mockValidator = new();

    public GetAiSettingsByUserIdQueryHandlerTests()
    {
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetAiSettingsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private GetAiSettingsByUserIdQueryHandler CreateHandler() =>
        new(_mockRepo.Object, _mockBuiltInPromptRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnExistingAiSettings_WhenAiSettingsExistForUser()
    {
        const string userId = "user-1";
        var existingSettings = new DomainAiSettings { Id = 1, UserId = userId, SelectedChatModelId = 3 };

        _mockRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(userId))
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
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(userId))
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
    public async Task Handle_ShouldPopulateBuiltInPrompts_InReturnedDto()
    {
        const string userId = "user-1";
        _mockRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(userId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = userId });

        var builtInPrompts = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("builtIn_Cover_Letter", "Template A"),
            BuiltInPrompt.Create("builtIn_Introduction", "Template B"),
        };
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(builtInPrompts);

        var result = await CreateHandler().Handle(new GetAiSettingsByUserIdQuery { UserId = userId }, CancellationToken.None);

        result.BuiltInPrompts.ShouldNotBeNull();
        result.BuiltInPrompts.Count.ShouldBe(2);
        result.BuiltInPrompts[0].Key.ShouldBe("builtIn_Cover_Letter");
        result.BuiltInPrompts[1].Key.ShouldBe("builtIn_Introduction");
    }
}
