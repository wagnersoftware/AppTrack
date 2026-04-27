using System.Text.Json;
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
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        var topicName = _configuration["ProjectScraping:TopicName"] ?? "project-scraping-events";

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(topicName);

        var payload = JsonSerializer.Serialize(new { PortalIds = portalIds.ToList() });
        await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);

        _logger.LogInformation("Published scraping completed event for portals {PortalIds}", string.Join(", ", portalIds));
    }
}
