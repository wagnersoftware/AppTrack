using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Validators;

public class UpdateRssMonitoringSettingsCommandValidatorTests
{
    private readonly UpdateRssMonitoringSettingsCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = ["dotnet"],
            PollIntervalMinutes = 60
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenKeywordsIsNull()
    {
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = null!,
            PollIntervalMinutes = 60
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
        var command = new UpdateRssMonitoringSettingsCommand
        {
            UserId = "user-1",
            Keywords = [],
            PollIntervalMinutes = interval
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }
}
