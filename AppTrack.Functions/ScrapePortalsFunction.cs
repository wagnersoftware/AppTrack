using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions timer trigger that scrapes all active project portals on a configurable schedule.
/// After scraping completes, publishes a signal to the Service Bus queue to trigger keyword matching.
/// </summary>
public sealed class ScrapePortalsFunction(
    IMediator mediator,
    IScrapingEventPublisher scrapingEventPublisher,
    ILogger<ScrapePortalsFunction> logger)
{
    [Function(nameof(ScrapePortalsFunction))]
    public async Task Run(
        [TimerTrigger("%ScrapeSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        logger.LogInformation("ScrapePortalsFunction started at {StartedAt}", startedAt);

        await mediator.Send(new ScrapePortalsCommand(), cancellationToken);
        await scrapingEventPublisher.PublishScrapingCompletedAsync([], cancellationToken);

        logger.LogInformation(
            "ScrapePortalsFunction completed. Duration: {Duration}ms",
            (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
    }
}
