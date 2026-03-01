using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppTrack.Api.Controllers;

[Route("api/job-applications")]
[ApiController]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET api/job-applications/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationDto>> Get([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var jobApplicationDto = await _mediator.Send(new GetJobApplicationByIdQuery { Id = id, UserId = userId });
        return Ok(jobApplicationDto);
    }

    // POST api/job-applications
    [HttpPost]
    [ProducesResponseType(typeof(JobApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationDto>> Post([FromBody] CreateJobApplicationCommand createJobApplicationCommand)
    {
        createJobApplicationCommand.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _mediator.Send(createJobApplicationCommand);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    // PUT api/job-applications/5
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JobApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationDto>> Put([FromRoute] int id, [FromBody] UpdateJobApplicationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        command.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // DELETE api/job-applications/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new DeleteJobApplicationCommand { Id = id, UserId = userId });
        return NoContent();
    }
}
