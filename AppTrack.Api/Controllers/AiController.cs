using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;
using AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.RenderPromptQuery;
using AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;
using AppTrack.Application.Features.JobApplications.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/ai")]
[ApiController]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET /api/ai/prompt-names
    [HttpGet("prompt-names")]
    [ProducesResponseType(typeof(GetPromptNamesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetPromptNamesDto>> GetPromptNames()
    {
        var result = await _mediator.Send(new GetPromptNamesQuery());
        return Ok(result);
    }

    // POST /api/ai/render-prompt
    [HttpPost("render-prompt")]
    [ProducesResponseType(typeof(RenderedPromptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RenderedPromptDto>> RenderPrompt([FromBody] RenderPromptQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // POST /api/ai/generate
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GeneratedAiTextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GeneratedAiTextDto>> Generate([FromBody] GenerateAiTextCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        return Ok(response);
    }

    // DELETE /api/ai/text/{id}
    [HttpDelete("text/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAiText([FromRoute] int id)
    {
        await _mediator.Send(new DeleteAiTextCommand { Id = id });
        return NoContent();
    }
}
