using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions timer trigger that scrapes all active project portals on a configurable schedule.
/// The schedule is read from the <c>ScrapeSchedule</c> configuration value (NCRONTAB format).
/// </summary>
public sealed class ScrapePortalsFunction(IMediator mediator, ILogger<ScrapePortalsFunction> logger)
{
    [Function(nameof(ScrapePortalsFunction))]
    public async Task Run(
        [TimerTrigger("%ScrapeSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        logger.LogInformation("ScrapePortalsFunction started at {StartedAt}", startedAt);

         await mediator.Send(new ScrapePortalsCommand(), cancellationToken);

        logger.LogInformation(
            "ScrapePortalsFunction completed. Duration: {Duration}ms",
            (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
    }
}
