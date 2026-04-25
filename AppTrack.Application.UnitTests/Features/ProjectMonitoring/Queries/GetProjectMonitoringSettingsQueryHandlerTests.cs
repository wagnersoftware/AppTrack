using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectMonitoringSettings;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Queries;

public class GetProjectMonitoringSettingsQueryHandlerTests
{
    private readonly Mock<IProjectMonitoringSettingsRepository> _mockRepo;

    public GetProjectMonitoringSettingsQueryHandlerTests()
    {
        _mockRepo = new Mock<IProjectMonitoringSettingsRepository>();
    }

    private GetProjectMonitoringSettingsQueryHandler CreateHandler() =>
        new(_mockRepo.Object);

    [Fact]
    public async Task Handle_ShouldReturnDefaults_WhenNoSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync((ProjectMonitoringSettings?)null);

        var result = await CreateHandler().Handle(
            new GetProjectMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBeEmpty();
        result.NotificationIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task Handle_ShouldReturnPersistedSettings_WhenSettingsExist()
    {
        _mockRepo.Setup(r => r.GetByUserIdAsync("user-1")).ReturnsAsync(
            new ProjectMonitoringSettings { UserId = "user-1", Keywords = ["dotnet"], NotificationIntervalMinutes = 30 });

        var result = await CreateHandler().Handle(
            new GetProjectMonitoringSettingsQuery { UserId = "user-1" }, CancellationToken.None);

        result.Keywords.ShouldBe(["dotnet"]);
        result.NotificationIntervalMinutes.ShouldBe(30);
    }
}
