using AppTrack.Application.Contracts.ProjectMonitoring;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class ServiceBusScrapingEventPublisher : IScrapingEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusScrapingEventPublisher> _logger;

    public ServiceBusScrapingEventPublisher(IConfiguration configuration, ILogger<ServiceBusScrapingEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishScrapingCompletedAsync(IEnumerable<int> portalIds, CancellationToken ct)
    {
        var connectionString = _configuration["ServiceBusConnection"];
        var queueName = _configuration["ScrapingCompletedQueueName"] ?? "scraping-completed";

        await using var client = new ServiceBusClient(connectionString);
        await using var sender = client.CreateSender(queueName);

        await sender.SendMessageAsync(new ServiceBusMessage("scraping-completed"), ct);

        _logger.LogInformation("Published scraping completed event to queue '{Queue}'", queueName);
    }
}
