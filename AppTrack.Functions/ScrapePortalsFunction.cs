using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions timer trigger that scrapes all active project portals on a randomised schedule.
/// Runs every 15 minutes but self-gates: only executes between 09:00–17:00 CET/CEST and only
/// when the randomised inter-run interval (90–150 minutes) has elapsed since the last run.
/// After scraping completes, publishes a signal to the Service Bus queue to trigger keyword matching.
/// </summary>
public sealed class ScrapePortalsFunction(
    IMediator mediator,
    IScrapingEventPublisher scrapingEventPublisher,
    IScrapingScheduleRepository scheduleRepository,
    TimeProvider timeProvider,
    ILogger<ScrapePortalsFunction> logger)
{
    private static readonly TimeZoneInfo CentralEuropean =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Central European Standard Time" : "Europe/Berlin");

    private const int WindowStartHour = 9;
    private const int WindowEndHour = 17;
    private const int MinIntervalMinutes = 90;
    private const int MaxIntervalMinutes = 150;

    [Function(nameof(ScrapePortalsFunction))]
    public async Task Run(
        [TimerTrigger("%ScrapeSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var nowOffset = timeProvider.GetUtcNow();
        var nowUtc = nowOffset.UtcDateTime;
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, CentralEuropean);

        if (localNow.Hour < WindowStartHour || localNow.Hour >= WindowEndHour)
        {
            logger.LogDebug("Outside operating window ({Start}:00–{End}:00 CET), skipping.", WindowStartHour, WindowEndHour);
            return;
        }

        var nextRunAfterUtc = await scheduleRepository.GetNextRunAfterAsync(cancellationToken);
        if (nextRunAfterUtc.HasValue && nowUtc < nextRunAfterUtc.Value)
        {
            logger.LogDebug("Next run scheduled for {Next} UTC, skipping.", nextRunAfterUtc.Value);
            return;
        }

        var startedAt = timeProvider.GetUtcNow();
        logger.LogInformation("ScrapePortalsFunction started at {StartedAt}", startedAt);

        Exception? scrapingException = null;
        try
        {
            await mediator.Send(new ScrapePortalsCommand(), cancellationToken);
            await scrapingEventPublisher.PublishScrapingCompletedAsync([], cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            scrapingException = ex;
            logger.LogError(ex, "ScrapePortalsFunction encountered an unexpected error.");
        }

        // Always update the schedule — even on failure — to avoid rapid retries on every 15-min tick.
        var nextRunUtc = scrapingException is null
            ? CalculateNextRunUtc(nowUtc)
            : nowUtc.AddMinutes(30);

        await scheduleRepository.SetNextRunAfterAsync(nextRunUtc, cancellationToken);

        if (scrapingException is null)
        {
            logger.LogInformation(
                "ScrapePortalsFunction completed. Duration: {Duration}ms. Next run after: {NextRun} UTC",
                (timeProvider.GetUtcNow() - startedAt).TotalMilliseconds,
                nextRunUtc);
        }
        else
        {
            logger.LogWarning("Retry scheduled for {NextRun} UTC (30-minute delay after failure).", nextRunUtc);
        }
    }

    private static DateTime CalculateNextRunUtc(DateTime nowUtc)
    {
        var delayMinutes = Random.Shared.Next(MinIntervalMinutes, MaxIntervalMinutes + 1);
        var candidateUtc = nowUtc.AddMinutes(delayMinutes);
        var candidateLocal = TimeZoneInfo.ConvertTimeFromUtc(candidateUtc, CentralEuropean);

        if (candidateLocal.Hour >= WindowEndHour || candidateLocal.Hour < WindowStartHour)
        {
            // Candidate falls outside window — push to next day at 09:00 + random offset up to 30 min
            var nextWindowStart = candidateLocal.Date
                .AddDays(candidateLocal.Hour >= WindowEndHour ? 1 : 0)
                .AddHours(WindowStartHour)
                .AddMinutes(Random.Shared.Next(0, 31));
            candidateUtc = TimeZoneInfo.ConvertTimeToUtc(nextWindowStart, CentralEuropean);
        }

        return candidateUtc;
    }
}
