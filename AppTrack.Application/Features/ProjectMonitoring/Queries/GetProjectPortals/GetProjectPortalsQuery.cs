using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Dto;

namespace AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;

public class GetProjectPortalsQuery : IRequest<List<ProjectPortalDto>>
{
}
