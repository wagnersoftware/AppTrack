using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;
using AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;
using AppTrack.Application.Features.RssFeeds.Dto;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssMonitoringSettings;
using AppTrack.Application.Features.RssFeeds.Queries.GetRssPortals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/rssfeeds")]
[ApiController]
[Authorize]
public class RssFeedController : ControllerBase
{
    private readonly IMediator _mediator;

    public RssFeedController(IMediator mediator) => _mediator = mediator;

    [HttpGet("portals")]
    [ProducesResponseType(typeof(List<RssPortalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RssPortalDto>>> GetPortals()
        => Ok(await _mediator.Send(new GetRssPortalsQuery()));

    [HttpPut("subscriptions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetSubscriptions([FromBody] SetRssSubscriptionsCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(RssMonitoringSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RssMonitoringSettingsDto>> GetSettings()
        => Ok(await _mediator.Send(new GetRssMonitoringSettingsQuery()));

    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateSettings([FromBody] UpdateRssMonitoringSettingsCommand command)
    {
        command.NotificationEmail = User.FindFirst("email")?.Value ?? string.Empty;
        await _mediator.Send(command);
        return NoContent();
    }
}
