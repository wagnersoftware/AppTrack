using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;
using AppTrack.Application.Features.ApplicationText.Dto;
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

    // PUT api/ai-settings/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AiSettingsDto>> UpdateAiSettings([FromRoute] int id, [FromBody] UpdateAiSettingsCommand updateAiSettingsCommand)
    {
        if (id != updateAiSettingsCommand.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        var result = await _mediator.Send(updateAiSettingsCommand);
        return Ok(result);
    }

    // GET /api/ai-settings/chat-models
    [HttpGet("chat-models")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ChatModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatModelDto>>> GetChatModels()
    {
        var result = await _mediator.Send(new GetChatModelsQuery());
        return Ok(result);
    }
}
