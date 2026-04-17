using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using AppTrack.Domain;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptNamesQueryHandlerTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();
    private readonly Mock<IValidator<GetPromptNamesQuery>> _mockValidator = new();

    public GetPromptNamesQueryHandlerTests()
    {
        // Default: no built-in prompts (keeps existing tests unaffected)
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetPromptNamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private GetPromptNamesQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockBuiltInPromptRepo.Object, _mockValidator.Object);

    [Fact]
    public async Task Handle_ShouldReturnNamesInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("Cover_Letter", "template A"));
        aiSettings.Prompts.Add(Prompt.Create("LinkedIn_Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Cover_Letter", "LinkedIn_Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPromptsAndNoBuiltIns()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsNotFound()
    {
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<GetPromptNamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "AI settings not found for this user.")]));

        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdWithPromptsReadOnlyAsync(UserId))
            .ReturnsAsync((DomainAiSettings?)null);

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnBuiltInPromptNames_WhenUserHasNoPrompts()
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

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["builtIn_Anschreiben", "builtIn_Vorstellung"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNamesThenBuiltInNames_WithUserNamesFirst()
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

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names[0].ShouldBe("My_Custom_Prompt");
        result.Names[^1].ShouldBe("builtIn_Anschreiben");
    }
}
