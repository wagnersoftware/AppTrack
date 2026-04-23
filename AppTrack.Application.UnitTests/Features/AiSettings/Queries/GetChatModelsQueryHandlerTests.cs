using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.AiSettings.Queries;

public class GetChatModelsQueryHandlerTests
{
    private readonly Mock<IChatModelRepository> _mockRepo = new();

    private GetChatModelsQueryHandler CreateHandler() => new(_mockRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveModels()
    {
        _mockRepo.Setup(r => r.GetAsync()).ReturnsAsync(new List<ChatModel>
        {
            new() { Id = 1, Name = "Active Model", ApiModelName = "gpt-active", IsActive = true },
            new() { Id = 2, Name = "Inactive Model", ApiModelName = "gpt-inactive", IsActive = false },
        });

        var result = await CreateHandler().Handle(new GetChatModelsQuery(), CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveModelsExist()
    {
        _mockRepo.Setup(r => r.GetAsync()).ReturnsAsync(new List<ChatModel>
        {
            new() { Id = 1, Name = "Inactive", ApiModelName = "gpt-old", IsActive = false },
        });

        var result = await CreateHandler().Handle(new GetChatModelsQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllModels_WhenAllAreActive()
    {
        _mockRepo.Setup(r => r.GetAsync()).ReturnsAsync(new List<ChatModel>
        {
            new() { Id = 1, Name = "Model A", ApiModelName = "gpt-a", IsActive = true },
            new() { Id = 2, Name = "Model B", ApiModelName = "gpt-b", IsActive = true },
        });

        var result = await CreateHandler().Handle(new GetChatModelsQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
    }
}
