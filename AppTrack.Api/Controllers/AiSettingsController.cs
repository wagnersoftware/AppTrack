using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/ai-settings")]
[ApiController]
[Authorize]
public class AiSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiSettingsController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET api/ai-settings/5
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiSettingsDto>> Get([FromRoute] string userId)
    {
        var query = new GetAiSettingsByUserIdQuery() { UserId = userId };
        var aiSettingsDto = await _mediator.Send(query);
        return Ok(aiSettingsDto);
    }

    // PUT api/ai-settings/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] UpdateAiSettingsCommand updateAiSettingsCommand)
    {
        if (id != updateAiSettingsCommand.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        await _mediator.Send(updateAiSettingsCommand);
        return NoContent();
    }
}
