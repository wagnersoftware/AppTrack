using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Queries;

public class GetRssMonitoringSettingsQueryHandlerTests
{
    private readonly Mock<IRssMonitoringSettingsRepository> _mockRepo;

    public GetRssMonitoringSettingsQueryHandlerTests()
    {
        _mockRepo = new Mock<IRssMonitoringSettingsRepository>();
    }

    private GetRssMonitoringSettingsQueryHandler CreateHandler() =>
        new(_mockRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnDefaults_WhenNoSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync((RssMonitoringSettings?)null);

        var result = await CreateHandler().Handle(
            new GetRssMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBeEmpty();
        result.PollIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_ShouldReturnPersistedSettings_WhenSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(
            new RssMonitoringSettings { UserId = "user-1", Keywords = ["dotnet"], PollIntervalMinutes = 30 });

        var result = await CreateHandler().Handle(
            new GetRssMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBe(["dotnet"]);
        result.PollIntervalMinutes.ShouldBe(30);
    }
}
