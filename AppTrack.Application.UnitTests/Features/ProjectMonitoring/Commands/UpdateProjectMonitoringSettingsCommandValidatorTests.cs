using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class UpdateProjectMonitoringSettingsCommandValidatorTests
{
    private readonly UpdateProjectMonitoringSettingsCommandValidator _validator = new();

    private static UpdateProjectMonitoringSettingsCommand BuildValidCommand() => new()
    {
        NotificationEmail = "user@example.com",
        Keywords = ["remote", ".NET"],
        NotificationIntervalMinutes = 60,
        NotifyByEmail = true
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var result = await _validator.TestValidateAsync(BuildValidCommand());
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenNotificationEmailIsEmpty()
    {
        var command = BuildValidCommand();
        command.NotificationEmail = string.Empty;

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.NotificationEmail);
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenKeywordsIsNull()
    {
        var command = BuildValidCommand();
        command.Keywords = null!;

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Keywords);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenKeywordsIsEmpty()
    {
        var command = BuildValidCommand();
        command.Keywords = [];

        var result = await _validator.TestValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public async Task Validate_ShouldHaveError_WhenNotificationIntervalMinutesIsOutOfRange(int minutes)
    {
        var command = BuildValidCommand();
        command.NotificationIntervalMinutes = minutes;

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.NotificationIntervalMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(720)]
    [InlineData(1440)]
    public async Task Validate_ShouldPass_WhenNotificationIntervalMinutesIsAllowedValue(int minutes)
    {
        var command = BuildValidCommand();
        command.NotificationIntervalMinutes = minutes;

        var result = await _validator.TestValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }
}
