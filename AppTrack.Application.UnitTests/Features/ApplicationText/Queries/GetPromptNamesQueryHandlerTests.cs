using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using AppTrack.Domain;
using Moq;
using Shouldly;
using DomainAiSettings = AppTrack.Domain.AiSettings;

namespace AppTrack.Application.UnitTests.Features.ApplicationText.Queries;

public class GetPromptNamesQueryHandlerTests
{
    private const string UserId = "user-1";
    private readonly Mock<IAiSettingsRepository> _mockAiSettingsRepo = new();
    private readonly Mock<IBuiltInPromptRepository> _mockBuiltInPromptRepo = new();

    public GetPromptNamesQueryHandlerTests()
    {
        // Default: no built-in prompts (keeps existing tests unaffected)
        _mockBuiltInPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<BuiltInPrompt>());
    }

    private GetPromptNamesQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockBuiltInPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnNamesInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("Cover_Letter", "template A"));
        aiSettings.Prompts.Add(Prompt.Create("LinkedIn_Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Cover_Letter", "LinkedIn_Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPromptsAndNoBuiltIns()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenAiSettingsNotFound()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync((DomainAiSettings?)null);

        await Should.ThrowAsync<BadRequestException>(() =>
            CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnBuiltInPromptNames_WhenUserHasNoPrompts()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var builtIns = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Default_Anschreiben", "template"),
            BuiltInPrompt.Create("Default_Vorstellung", "template"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(builtIns);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Default_Anschreiben", "Default_Vorstellung"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNamesThenBuiltInNames_WithUserNamesFirst()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("My_Custom_Prompt", "template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var builtIns = new List<BuiltInPrompt>
        {
            BuiltInPrompt.Create("Default_Anschreiben", "template"),
        };
        _mockBuiltInPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(builtIns);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names[0].ShouldBe("My_Custom_Prompt");
        result.Names[^1].ShouldBe("Default_Anschreiben");
    }

}
