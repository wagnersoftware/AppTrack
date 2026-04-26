using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectPortals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/projectmonitoring")]
[ApiController]
[Authorize]
public class ProjectMonitoringController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectMonitoringController(IMediator mediator) => _mediator = mediator;

    [HttpGet("portals")]
    [ProducesResponseType(typeof(List<ProjectPortalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ProjectPortalDto>>> GetPortals()
        => Ok(await _mediator.Send(new GetProjectPortalsQuery()));
}
