using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;

public class GetRssPortalsQuery : IRequest<List<RssPortalDto>>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
