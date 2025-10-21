using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/Ai-settings")]
[ApiController]
[Authorize]
public class AiSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiSettingsController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // PUT api/ai-settings/5
    [HttpPut("{id}", Name = "UpdateAiSettings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
