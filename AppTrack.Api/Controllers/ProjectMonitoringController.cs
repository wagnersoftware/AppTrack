using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;
using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using AppTrack.Application.Features.ProjectMonitoring.Queries.GetProjectMonitoringSettings;
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

    [HttpPut("subscriptions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetSubscriptions([FromBody] SetPortalSubscriptionsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(ProjectMonitoringSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectMonitoringSettingsDto>> GetSettings()
        => Ok(await _mediator.Send(new GetProjectMonitoringSettingsQuery()));

    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateSettings([FromBody] UpdateProjectMonitoringSettingsCommand command)
    {
        command.NotificationEmail = User.FindFirst("email")?.Value ?? string.Empty;
        await _mediator.Send(command);
        return NoContent();
    }
}
