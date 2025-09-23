using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsController(IMediator mediator)
    {
        this._mediator = mediator;
    }
    // GET: api/<JobApplicationsController>
    [HttpGet]
    public async Task<ActionResult<List<JobApplicationDto>>> Get()
    {
        var jobApplicationDtos = await _mediator.Send(new GetJobApplicationsQuery());
        return Ok(jobApplicationDtos);
    }

    // GET api/<JobApplicationsController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationDto>> Get(int id)
    {
        var jobApplicationDto = await _mediator.Send(new GetJobApplicationByIdQuery() { Id = id});
        return Ok(jobApplicationDto);
    }

    // POST api/<JobApplicationsController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDto>> Post(CreateJobApplicationCommand createJobApplicationCommand)
    {
        var response = await _mediator.Send(createJobApplicationCommand);
        return CreatedAtAction(nameof(Get), response );
    }

    // PUT api/<JobApplicationsController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> Put(UpdateJobApplicationCommand updateJobApplicationCommand)
    {
        await _mediator.Send(updateJobApplicationCommand);
        return NoContent();
    }

    // DELETE api/<JobApplicationsController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        await _mediator.Send(new DeleteJobApplicationCommand() { Id = id });
        return NoContent();
    }
}
