using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions timer trigger that sends email notifications for unnotified project matches.
/// Runs frequently; per-user notification intervals are enforced by the handler.
/// </summary>
public sealed class SendNotificationsFunction(IMediator mediator, ILogger<SendNotificationsFunction> logger)
{
    [Function(nameof(SendNotificationsFunction))]
    public async Task Run(
        [TimerTrigger("%NotificationSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("SendNotificationsFunction started.");
        await mediator.Send(new SendProjectNotificationsCommand(), cancellationToken);
        logger.LogInformation("SendNotificationsFunction completed.");
    }
}
