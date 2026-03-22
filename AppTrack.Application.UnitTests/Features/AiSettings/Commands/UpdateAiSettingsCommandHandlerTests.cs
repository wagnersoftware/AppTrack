using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Commands;

public class UpdateAiSettingsCommandHandlerTests
{
    private readonly UpdateAiSettingsCommandHandler _handler;

    public UpdateAiSettingsCommandHandlerTests()
    {
        var mockRepo = MockAiSettingsRepository.GetMock();
        _handler = new UpdateAiSettingsCommandHandler(mockRepo.Object);
    }

    [Fact]
    public async Task Handle_WithPrompts_ShouldReturnDtoWithPrompts()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
            Prompts =
            [
                new PromptDto { Name = "My Prompt", PromptTemplate = "Hello {name}" }
            ]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeOfType<AiSettingsDto>();
        result.Prompts.Count.ShouldBe(1);
        result.Prompts[0].Name.ShouldBe("My Prompt");
    }

    [Fact]
    public async Task Handle_WithEmptyPrompts_ShouldReturnEmptyPromptsList()
    {
        var command = new UpdateAiSettingsCommand
        {
            Id = 1,
            UserId = "user1",
            SelectedChatModelId = 1,
            Prompts = []
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Prompts.ShouldBeEmpty();
    }
}
