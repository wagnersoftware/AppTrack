using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.PollProjects;
using AppTrack.Application.Features.ProjectMonitoring.Commands.ScrapePortals;

namespace AppTrack.Api.BackgroundServices;

public class ProjectMonitoringBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProjectMonitoringBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ScrapePortalsCommand(), stoppingToken);
                await mediator.Send(new PollProjectsCommand(), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during project monitoring");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
