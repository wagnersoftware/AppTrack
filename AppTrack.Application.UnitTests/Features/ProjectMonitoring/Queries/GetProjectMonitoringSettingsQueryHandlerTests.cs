using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectMonitoringSettings;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Queries;

public class GetProjectMonitoringSettingsQueryHandlerTests
{
    private readonly Mock<IProjectMonitoringSettingsRepository> _repository = new();

    private GetProjectMonitoringSettingsQueryHandler CreateHandler()
        => new(_repository.Object);

    private static GetProjectMonitoringSettingsQuery BuildQuery(string userId = "user-1")
        => new() { UserId = userId };

    [Fact]
    public async Task Handle_ShouldReturnDefaultDto_WhenRepositoryReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync((ProjectMonitoringSettings?)null);

        // Act
        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Keywords.ShouldBeEmpty();
        result.NotificationIntervalMinutes.ShouldBe(60);
        result.NotifyByEmail.ShouldBeFalse();
        result.NotificationEmail.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Handle_ShouldMapAllFields_WhenRepositoryReturnsSettings()
    {
        // Arrange
        var settings = new ProjectMonitoringSettings
        {
            UserId = "user-1",
            Keywords = new List<string> { ".NET", "remote" },
            NotificationIntervalMinutes = 120,
            NotifyByEmail = true,
            NotificationEmail = "user@example.com"
        };

        _repository.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync(settings);

        // Act
        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Keywords.ShouldBe(new List<string> { ".NET", "remote" });
        result.NotificationIntervalMinutes.ShouldBe(120);
        result.NotifyByEmail.ShouldBeTrue();
        result.NotificationEmail.ShouldBe("user@example.com");
    }

    [Fact]
    public async Task Handle_ShouldMapNotificationEmail_WhenRepositoryReturnsSettings()
    {
        // Arrange — NotificationEmail is the branch change; assert it explicitly
        var settings = new ProjectMonitoringSettings
        {
            UserId = "user-1",
            Keywords = new List<string>(),
            NotificationIntervalMinutes = 60,
            NotifyByEmail = false,
            NotificationEmail = "notifications@company.org"
        };

        _repository.Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync(settings);

        // Act
        var result = await CreateHandler().Handle(BuildQuery(), CancellationToken.None);

        // Assert
        result.NotificationEmail.ShouldBe("notifications@company.org");
    }
}
