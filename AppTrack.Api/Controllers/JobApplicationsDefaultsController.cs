using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace AppTrack.Api.Controllers;

[Route("api/job-application-defaults")]
[ApiController]
[Authorize]
public class JobApplicationsDefaultsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsDefaultsController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // PUT api/job-application-defaults/5
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JobApplicationDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationDefaultsDto>> Put([FromRoute] int id, [FromBody] UpdateJobApplicationDefaultsCommand updateJobApplicationDefaultsCommand)
    {
        if (id != updateJobApplicationDefaultsCommand.Id)
        {
            return BadRequest("Route ID and body ID do not match.");
        }

        updateJobApplicationDefaultsCommand.UserId = User.GetObjectId()!;
        var result = await _mediator.Send(updateJobApplicationDefaultsCommand);
        return Ok(result);
    }
}
