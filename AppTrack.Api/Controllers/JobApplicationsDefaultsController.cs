using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
        //todo für aktuellen User(me), sobald Authorisierung implementiert -> var userId = _userContext.UserId; // aus JWT / ClaimsPrincipal
        var jobApplicationDetailsDto = await _mediator.Send(new GetJobApplicationDefaultsByUserIdQuery() { UserId = userId });
        return Ok(jobApplicationDetailsDto);
    }

    // GET api/<JobApplicationsController>/5
    [HttpPut("{id}", Name = "UpdateJobApplicationDefaults")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> Put([FromRoute] int id, [FromBody] UpdateJobApplicationDefaultsCommand updateJobApplicationDefaultsCommand)
    {
        if (id != updateJobApplicationDefaultsCommand.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        await _mediator.Send(updateJobApplicationDefaultsCommand);
        return NoContent();
    }
}
