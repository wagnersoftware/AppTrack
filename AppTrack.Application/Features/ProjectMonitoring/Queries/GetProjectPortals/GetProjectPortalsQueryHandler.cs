using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Dto;

namespace AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;

public class GetProjectPortalsQueryHandler : IRequestHandler<GetProjectPortalsQuery, List<ProjectPortalDto>>
{
    private readonly IProjectPortalRepository _portalRepository;
    private readonly IUserPortalSubscriptionRepository _subscriptionRepository;

    public GetProjectPortalsQueryHandler(
        IProjectPortalRepository portalRepository,
        IUserPortalSubscriptionRepository subscriptionRepository)
    {
        _portalRepository = portalRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<List<ProjectPortalDto>> Handle(GetProjectPortalsQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();
        var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(request.UserId);
        var activePortalIds = userSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.ProjectPortalId)
            .ToHashSet();

        return portals
            .Select(p => new ProjectPortalDto(p.Id, p.Name, p.Url, activePortalIds.Contains(p.Id)))
            .ToList();
    }
}
