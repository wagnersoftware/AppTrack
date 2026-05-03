using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.CleanupProjectData;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions timer trigger that deletes scraped projects, user matches, and processed
/// project items older than the configured retention window (60 days).
/// </summary>
public sealed class CleanupFunction(IMediator mediator, ILogger<CleanupFunction> logger)
{
    [Function(nameof(CleanupFunction))]
    public async Task Run([TimerTrigger("%CleanupSchedule%")] TimerInfo timerInfo, CancellationToken ct)
    {
        logger.LogInformation("CleanupFunction triggered at {Time}", DateTime.UtcNow);
        await mediator.Send(new CleanupProjectDataCommand(), ct);
        logger.LogInformation("CleanupFunction completed");
    }
}
