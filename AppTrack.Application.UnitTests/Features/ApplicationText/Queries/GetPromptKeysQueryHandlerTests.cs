using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptKeysQuery;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptKeysQueryHandlerTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();
    private readonly Mock<IValidator<GetPromptKeysQuery>> _mockValidator = new();

    public GetPromptKeysQueryHandlerTests()
    {
        // Default: no built-in prompts (keeps existing tests unaffected)
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetPromptKeysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private GetPromptKeysQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockBuiltInPromptRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnKeysInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("Cover_Letter", "template A"));
        aiSettings.Prompts.Add(Prompt.Create("LinkedIn_Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptKeysQuery { UserId = UserId }, CancellationToken.None);

        result.Keys.ShouldBe(["Cover_Letter", "LinkedIn_Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPromptsAndNoBuiltIns()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var result = await CreateHandler().Handle(new GetPromptKeysQuery { UserId = UserId }, CancellationToken.None);

        result.Keys.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsNotFound()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetPromptKeysQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "AI settings not found for this user.")]));

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync((DomainAiSettings?)null);

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new GetPromptKeysQuery { UserId = UserId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnBuiltInPromptKeys_WhenUserHasNoPrompts()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var builtIns = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("builtIn_Anschreiben", "template"),
            BuiltInPrompt.Create("builtIn_Vorstellung", "template"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(builtIns);

        var result = await CreateHandler().Handle(new GetPromptKeysQuery { UserId = UserId }, CancellationToken.None);

        result.Keys.ShouldBe(["builtIn_Anschreiben", "builtIn_Vorstellung"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserKeysThenBuiltInKeys_WithUserKeysFirst()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("My_Custom_Prompt", "template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(aiSettings);

        var builtIns = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("builtIn_Anschreiben", "template"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(builtIns);

        var result = await CreateHandler().Handle(new GetPromptKeysQuery { UserId = UserId }, CancellationToken.None);

        result.Keys[0].ShouldBe("My_Custom_Prompt");
        result.Keys[^1].ShouldBe("builtIn_Anschreiben");
    }
}
