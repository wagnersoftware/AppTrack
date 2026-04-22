using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public sealed class UpdateRssMonitoringSettingsCommandValidator
    : AbstractValidator<UpdateRssMonitoringSettingsCommand>
{
    public UpdateRssMonitoringSettingsCommandValidator()
    {
        RuleFor(x => x.NotificationEmail).NotEmpty();
        RuleFor(x => x.Keywords).NotNull();
        RuleFor(x => x.PollIntervalMinutes).InclusiveBetween(5, 1440);
    }
}
