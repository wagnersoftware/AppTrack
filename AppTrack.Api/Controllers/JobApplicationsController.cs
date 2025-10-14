using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsController(IMediator mediator)
    {
        this._mediator = mediator;
    }
    // GET: api/<JobApplicationsController>
    [HttpGet("forUser/{userId}")]
    [ProducesResponseType(typeof(List<JobApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<JobApplicationDto>>> Get(string userId)
    {
        var jobApplicationDtos = await _mediator.Send(new GetJobApplicationsForUserQuery() { UserId = userId });
        return Ok(jobApplicationDtos);
    }

    // GET api/<JobApplicationsController>/5
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDto>> Get(int id)
    {
        var jobApplicationDto = await _mediator.Send(new GetJobApplicationByIdQuery() { Id = id });
        return Ok(jobApplicationDto);
    }

    // POST api/<JobApplicationsController>
    [HttpPost]
    [ProducesResponseType(typeof(JobApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobApplicationDto>> Post(CreateJobApplicationCommand createJobApplicationCommand)
    {
        var response = await _mediator.Send(createJobApplicationCommand);
        return CreatedAtAction(nameof(Post), response);
    }

    // PUT api/<JobApplicationsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobApplicationDto))]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDto>> Put(UpdateJobApplicationCommand updateJobApplicationCommand)
    {
        var jobApplicationDto = await _mediator.Send(updateJobApplicationCommand);
        return Ok(jobApplicationDto);
    }

    // DELETE api/<JobApplicationsController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _mediator.Send(new DeleteJobApplicationCommand() { Id = id });
        return NoContent();
    }
}
