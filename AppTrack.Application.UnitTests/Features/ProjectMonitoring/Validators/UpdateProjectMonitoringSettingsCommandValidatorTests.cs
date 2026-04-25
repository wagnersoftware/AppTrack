using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Validators;

public class UpdateProjectMonitoringSettingsCommandValidatorTests
{
    private readonly UpdateProjectMonitoringSettingsCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = ["dotnet"],
            NotificationIntervalMinutes = 60,
            PollIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenKeywordsIsNull()
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = null!,
            NotificationIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public async Task Validate_ShouldFail_WhenIntervalIsOutOfRange(int interval)
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = [],
            NotificationIntervalMinutes = interval
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNotificationEmailIsEmpty()
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = string.Empty,
            Keywords = [],
            NotificationIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(1441)]
    public async Task Validate_ShouldFail_WhenPollIntervalIsOutOfRange(int pollInterval)
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = [],
            NotificationIntervalMinutes = 60,
            PollIntervalMinutes = pollInterval
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(1440)]
    public async Task Validate_ShouldPass_WhenPollIntervalIsOnBoundary(int pollInterval)
    {
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            UserId = "user-1",
            NotificationEmail = "user@example.com",
            Keywords = [],
            NotificationIntervalMinutes = 60,
            PollIntervalMinutes = pollInterval
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
