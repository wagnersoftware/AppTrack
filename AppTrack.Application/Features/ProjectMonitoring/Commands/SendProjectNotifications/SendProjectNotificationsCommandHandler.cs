using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Models.Email;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;

public class SendProjectNotificationsCommandHandler : IRequestHandler<SendProjectNotificationsCommand, Unit>
{
    private readonly IUserProjectMatchRepository _matchRepository;
    private readonly IProjectMonitoringSettingsRepository _settingsRepository;
    private readonly IEmailSender _emailSender;

    public SendProjectNotificationsCommandHandler(
        IUserProjectMatchRepository matchRepository,
        IProjectMonitoringSettingsRepository settingsRepository,
        IEmailSender emailSender)
    {
        _matchRepository = matchRepository;
        _settingsRepository = settingsRepository;
        _emailSender = emailSender;
    }

    public async Task<Unit> Handle(SendProjectNotificationsCommand request, CancellationToken cancellationToken)
    {
        var unnotified = await _matchRepository.GetUnnotifiedAsync(cancellationToken);
        if (unnotified.Count == 0)
            return Unit.Value;

        var byUser = unnotified.GroupBy(m => m.UserId);

        foreach (var userGroup in byUser)
        {
            var userId = userGroup.Key;
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings is null)
                continue;

            var isIntervalReached = settings.LastNotifiedAt is null ||
                DateTime.UtcNow >= settings.LastNotifiedAt.Value.AddMinutes(settings.NotificationIntervalMinutes);

            if (!isIntervalReached)
                continue;

            var matches = userGroup.ToList();
            var body = string.Join("\n", matches.Select(m =>
                $"- {m.ScrapedProject.Title} ({m.ScrapedProject.ProjectPortal?.Name ?? string.Empty}): {m.ScrapedProject.Url}"));

            var email = new EmailMessage
            {
                To = settings.NotificationEmail,
                Subject = $"{matches.Count} new job(s) discovered",
                Body = $"The following jobs matched your keywords:\n\n{body}"
            };

            var sent = await _emailSender.SendEmail(email);
            if (!sent)
                continue;

            await _matchRepository.MarkNotifiedAsync(matches.Select(m => m.Id), cancellationToken);

            settings.LastNotifiedAt = DateTime.UtcNow;
            await _settingsRepository.UpdateAsync(settings);
        }

        return Unit.Value;
    }
}
