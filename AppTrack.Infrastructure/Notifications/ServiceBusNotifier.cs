using System.Text.Json;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class ServiceBusNotifier : IRssMatchNotifier
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusNotifier> _logger;

    public ServiceBusNotifier(IConfiguration configuration, ILogger<ServiceBusNotifier> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, string userEmail, List<RssJobApplicationData> matches, CancellationToken ct)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        var queueName = _configuration["RssNotification:QueueName"] ?? "rss-matches";

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(queueName);

        var payload = JsonSerializer.Serialize(new { UserId = userId, Matches = matches });
        await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);

        _logger.LogInformation("Published {Count} RSS matches for user {UserId} to Service Bus",
            matches.Count, userId);
    }
}
