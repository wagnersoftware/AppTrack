using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query;
using AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;
using AppTrack.Application.Features.JobApplications.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApplicationTextController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationTextController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    //todo move this to job application controller
    [HttpPost(Name = "GenerateApplicationText")]
    [ProducesResponseType(typeof(GeneratedApplicationTextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratedApplicationTextDto>> GenerateApplicationText([FromBody] GenerateApplicationTextCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet(Name = "GeneratePrompt")]
    [ProducesResponseType(typeof(GeneratedPromptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneratedPromptDto>> GeneratePrompt([FromBody] GeneratePromptQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
