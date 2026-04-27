using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AppTrack.Functions;

/// <summary>
/// Azure Functions Service Bus trigger that runs keyword matching for all users
/// after a scraping cycle completes.
/// </summary>
public sealed class MatchProjectsFunction(IMediator mediator, ILogger<MatchProjectsFunction> logger)
{
    [Function(nameof(MatchProjectsFunction))]
    public async Task Run(
        [ServiceBusTrigger("%ScrapingCompletedQueueName%", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("MatchProjectsFunction triggered by Service Bus message {MessageId}", message.MessageId);
        await mediator.Send(new MatchProjectsCommand(), cancellationToken);
        logger.LogInformation("MatchProjectsFunction completed.");
    }
}
