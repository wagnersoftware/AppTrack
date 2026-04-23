using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.RssFeeds.Validators;

public class SetRssSubscriptionsCommandValidatorTests
{
    private readonly SetRssSubscriptionsCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(1, true)]
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSubscriptionsIsNull()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = null!
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPortalIdIsZero()
    {
        var command = new SetRssSubscriptionsCommand
        {
            UserId = "user-1",
            Subscriptions = [new(0, true)]
        };
        var result = await _sut.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
    }
}
