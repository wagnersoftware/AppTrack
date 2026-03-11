using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET: api/users/job-applications
    [HttpGet("job-applications")]
    [ProducesResponseType(typeof(List<JobApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<JobApplicationDto>>> GetJobApplications()
    {
        var jobApplicationDtos = await _mediator.Send(new GetJobApplicationsForUserQuery());
        return Ok(jobApplicationDtos);
    }

    // GET api/users/ai-settings
    [HttpGet("ai-settings")]
    [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AiSettingsDto>> GetAiSettings()
    {
        var aiSettingsDto = await _mediator.Send(new GetAiSettingsByUserIdQuery());
        return Ok(aiSettingsDto);
    }

    // GET api/users/job-application-defaults
    [HttpGet("job-application-defaults")]
    [ProducesResponseType(typeof(JobApplicationDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationDefaultsDto>> GetJobApplicationDefaults()
    {
        var jobApplicationDetailsDto = await _mediator.Send(new GetJobApplicationDefaultsByUserIdQuery());
        return Ok(jobApplicationDetailsDto);
    }
}
