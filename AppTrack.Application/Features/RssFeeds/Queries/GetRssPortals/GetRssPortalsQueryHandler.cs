using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Dto;

namespace AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;

public class GetRssPortalsQueryHandler : IRequestHandler<GetRssPortalsQuery, List<RssPortalDto>>
{
    private readonly IRssPortalRepository _portalRepository;
    private readonly IUserRssSubscriptionRepository _subscriptionRepository;

    public GetRssPortalsQueryHandler(
        IRssPortalRepository portalRepository,
        IUserRssSubscriptionRepository subscriptionRepository)
    {
        _portalRepository = portalRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<List<RssPortalDto>> Handle(GetRssPortalsQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();
        var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(request.UserId);
        var activePortalIds = userSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.RssPortalId)
            .ToHashSet();

        return portals
            .Select(p => new RssPortalDto(p.Id, p.Name, activePortalIds.Contains(p.Id)))
            .ToList();
    }
}
