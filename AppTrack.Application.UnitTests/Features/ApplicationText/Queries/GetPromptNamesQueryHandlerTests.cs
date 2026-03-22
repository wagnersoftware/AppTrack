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
    private readonly Mock<IDefaultPromptRepository> _mockDefaultPromptRepo = new();

    public GetPromptNamesQueryHandlerTests()
    {
        // Default: no default prompts (keeps existing tests unaffected)
        _mockDefaultPromptRepo
            .Setup(r => r.GetAsync())
            .ReturnsAsync(new List<DefaultPrompt>());
    }

    private GetPromptNamesQueryHandler CreateHandler() =>
        new(_mockAiSettingsRepo.Object, _mockDefaultPromptRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnNamesInInsertionOrder_WhenAiSettingsHaveMultiplePrompts()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("Cover Letter", "template A"));
        aiSettings.Prompts.Add(Prompt.Create("LinkedIn Message", "template B"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Cover Letter", "LinkedIn Message"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAiSettingsHaveNoPromptsAndNoDefaults()
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
    public async Task Handle_ShouldReturnDefaultPromptNames_WhenUserHasNoPrompts()
    {
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(new DomainAiSettings { Id = 1, UserId = UserId });

        var defaults = new List<DefaultPrompt>
        {
            DefaultPrompt.Create("Anschreiben", "template", "de"),
            DefaultPrompt.Create("Vorstellung", "template", "de"),
        };
        _mockDefaultPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names.ShouldBe(["Anschreiben", "Vorstellung"]);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNamesThenDefaultNames_WithUserNamesFirst()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("My Custom Prompt", "template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var defaults = new List<DefaultPrompt>
        {
            DefaultPrompt.Create("Anschreiben", "template", "de"),
        };
        _mockDefaultPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        result.Names[0].ShouldBe("My Custom Prompt");
        result.Names[^1].ShouldBe("Anschreiben");
    }

    [Fact]
    public async Task Handle_ShouldDeduplicatePromptNames_CaseInsensitively()
    {
        var aiSettings = new DomainAiSettings { Id = 1, UserId = UserId };
        aiSettings.Prompts.Add(Prompt.Create("anschreiben", "user template"));
        _mockAiSettingsRepo
            .Setup(r => r.GetByUserIdIncludePromptParameterAsync(UserId))
            .ReturnsAsync(aiSettings);

        var defaults = new List<DefaultPrompt>
        {
            DefaultPrompt.Create("Anschreiben", "default template", "de"),
            DefaultPrompt.Create("Vorstellung", "template", "de"),
        };
        _mockDefaultPromptRepo.Setup(r => r.GetAsync()).ReturnsAsync(defaults);

        var result = await CreateHandler().Handle(new GetPromptNamesQuery { UserId = UserId }, CancellationToken.None);

        // "Anschreiben" from defaults is suppressed; "anschreiben" from user + "Vorstellung" remain
        result.Names.Count.ShouldBe(2);
        result.Names.ShouldContain("anschreiben");
        result.Names.ShouldContain("Vorstellung");
    }
}
