using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Dto;

namespace AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;

public class GetProjectPortalsQueryHandler : IRequestHandler<GetProjectPortalsQuery, List<ProjectPortalDto>>
{
    private readonly IProjectPortalRepository _portalRepository;

    public GetProjectPortalsQueryHandler(IProjectPortalRepository portalRepository)
    {
        _portalRepository = portalRepository;
    }

    public async Task<List<ProjectPortalDto>> Handle(GetProjectPortalsQuery request, CancellationToken cancellationToken)
    {
        var portals = await _portalRepository.GetAllActiveAsync();
        return portals
            .Select(p => new ProjectPortalDto(p.Id, p.Name, p.Url))
            .ToList();
    }
}
