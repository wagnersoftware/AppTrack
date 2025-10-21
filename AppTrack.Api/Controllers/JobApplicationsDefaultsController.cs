using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    // GET api/job-application-defaults/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
