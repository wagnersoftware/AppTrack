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
using System.Security.Claims;

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

    // GET: api/users/5/job-applications
    [HttpGet("{userId}/job-applications")]
    [ProducesResponseType(typeof(List<JobApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<JobApplicationDto>>> GetJobApplications([FromRoute]string userId)
    {
        if (userId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        var jobApplicationDtos = await _mediator.Send(new GetJobApplicationsForUserQuery() { UserId = userId });
        return Ok(jobApplicationDtos);
    }

    // GET api/users/5/ai-settings
    [HttpGet("{userId}/ai-settings")]
    [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AiSettingsDto>> GetAiSettings([FromRoute] string userId)
    {
        if (userId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        var aiSettingsDto = await _mediator.Send(new GetAiSettingsByUserIdQuery() { UserId = userId});
        return Ok(aiSettingsDto);
    }

    // GET api/users/5/job-application-defaults
    [HttpGet("{userId}/job-application-defaults")]
    [ProducesResponseType(typeof(JobApplicationDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JobApplicationDefaultsDto>> GetJobApplicationDefaults([FromRoute]string userId)
    {
        if (userId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        var jobApplicationDetailsDto = await _mediator.Send(new GetJobApplicationDefaultsByUserIdQuery() { UserId = userId });
        return Ok(jobApplicationDetailsDto);
    }
}
