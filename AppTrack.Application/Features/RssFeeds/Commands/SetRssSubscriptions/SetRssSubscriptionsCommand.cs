using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public List<RssSubscriptionItemDto> Subscriptions { get; set; } = [];
}
