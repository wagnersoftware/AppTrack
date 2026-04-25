using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Application.Models.Email;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class DirectEmailProjectNotifier : IProjectMatchNotifier
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<DirectEmailProjectNotifier> _logger;

    public DirectEmailProjectNotifier(IEmailSender emailSender, ILogger<DirectEmailProjectNotifier> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, string userEmail, List<ScrapedProjectData> matches, CancellationToken ct)
    {
        var body = string.Join("\n", matches.Select(m => $"- {m.Position} ({m.PortalName}): {m.Url}"));
        var email = new EmailMessage
        {
            To = userEmail,
            Subject = $"{matches.Count} new job(s) discovered",
            Body = $"The following jobs matched your keywords:\n\n{body}"
        };

        var sent = await _emailSender.SendEmail(email);
        if (!sent)
            _logger.LogWarning("Failed to send project match notification email for user {UserId}", userId);
    }
}
