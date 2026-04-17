using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;
using AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.JobApplications.Dto;
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

    // PUT api/ai-settings/5
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

    // POST /api/ai-settings/generate-application-text
    [HttpPost("generate-application-text")]
    [ProducesResponseType(typeof(GeneratedAiTextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GeneratedAiTextDto>> GenerateApplicationText([FromBody] GenerateAiTextCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return Ok(response);
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

    // DELETE /api/ai-settings/ai-text/{id}
    [HttpDelete("ai-text/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAiText([FromRoute] int id)
    {
        await _mediator.Send(new DeleteAiTextCommand { Id = id });
        return NoContent();
    }
}
