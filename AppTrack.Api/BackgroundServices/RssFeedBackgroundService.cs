using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;

namespace AppTrack.Api.BackgroundServices;

public class RssFeedBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RssFeedBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new PollRssFeedsCommand(), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during RSS feed polling");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
