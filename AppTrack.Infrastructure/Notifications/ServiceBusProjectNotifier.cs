using System.Text.Json;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppTrack.Infrastructure.Notifications;

public class ServiceBusProjectNotifier : IProjectMatchNotifier
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusProjectNotifier> _logger;

    public ServiceBusProjectNotifier(IConfiguration configuration, ILogger<ServiceBusProjectNotifier> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(string userId, string userEmail, List<ScrapedProjectData> matches, CancellationToken ct)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        var queueName = _configuration["ProjectNotification:QueueName"] ?? "project-matches";

        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(queueName);

        var payload = JsonSerializer.Serialize(new { UserId = userId, Matches = matches });
        await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);

        _logger.LogInformation("Published {Count} project matches for user {UserId} to Service Bus",
            matches.Count, userId);
    }
}
