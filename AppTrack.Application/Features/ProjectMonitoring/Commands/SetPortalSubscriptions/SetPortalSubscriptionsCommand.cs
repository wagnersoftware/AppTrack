using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;

public class SetPortalSubscriptionsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public List<PortalSubscriptionItemDto> Subscriptions { get; set; } = [];
}
