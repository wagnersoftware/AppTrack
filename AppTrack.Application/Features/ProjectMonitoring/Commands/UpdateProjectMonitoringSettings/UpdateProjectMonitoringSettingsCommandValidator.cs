using FluentValidation;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;

public sealed class UpdateProjectMonitoringSettingsCommandValidator
    : AbstractValidator<UpdateProjectMonitoringSettingsCommand>
{
    public UpdateProjectMonitoringSettingsCommandValidator()
    {
        RuleFor(x => x.NotificationEmail).NotEmpty();
        RuleFor(x => x.Keywords).NotNull();
        RuleFor(x => x.NotificationIntervalMinutes).InclusiveBetween(5, 1440);
        RuleFor(x => x.PollIntervalMinutes).InclusiveBetween(5, 1440);
    }
}
