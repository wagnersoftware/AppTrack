using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.GenerateApplicationText;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateAiSettings([FromRoute] int id, [FromBody] UpdateAiSettingsCommand updateAiSettingsCommand)
    {
        if (id != updateAiSettingsCommand.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        await _mediator.Send(updateAiSettingsCommand);
        return NoContent();
    }

    // POST /api/ai-settings/generate-application-text
    [HttpPost("generate-application-text")]
    [ProducesResponseType(typeof(GeneratedApplicationTextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GeneratedApplicationTextDto>> GenerateApplicationText([FromBody] GenerateApplicationTextCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return Ok(response);
    }

    // GET /api/ai-settings/chat-models
    [HttpGet("chat-models")]
    [ProducesResponseType(typeof(List<ChatModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ChatModelDto>>> GetChatModels()
    {
        var result = await _mediator.Send(new GetChatModelsQuery());
        return Ok(result);
    }
}
