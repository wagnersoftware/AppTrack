using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaultsByUserId;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobApplicationsDefaultsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsDefaultsController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET api/<JobApplicationsController>/5
    [HttpGet("{userId}", Name = "GetJobApplicationDefaultsForUser")]
    [ProducesResponseType(typeof(JobApplicationDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDefaultsDto>> GetForUser(string userId)
    {
        var jobApplicationDetailsDto = await _mediator.Send(new GetJobApplicationDefaultsByUserIdQuery() { UserId = userId });
        return Ok(jobApplicationDetailsDto);
    }

    // GET api/<JobApplicationsController>/5
    [HttpPut("{userId}", Name = "UpdateJobApplicationDefaultsForUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> UpdateForUser([FromRoute] string userId, [FromBody] UpdateJobApplicationDefaultsByUserIdCommand updateJobApplicationDefaultsByUserIdCommand)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        updateJobApplicationDefaultsByUserIdCommand.UserId = userId;
        await _mediator.Send(updateJobApplicationDefaultsByUserIdCommand);
        return NoContent();
    }
}
