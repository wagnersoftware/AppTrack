using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

public class JobApplicationsDefaultController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobApplicationsDefaultController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // GET api/<JobApplicationsController>/5
    [HttpGet("{userId}", Name = "GetJobApplicationDefaultsByUserId")]
    [ProducesResponseType(typeof(JobApplicationDefaultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationDefaultsDto>> GetByUserId(int userId)
    {
        var jobApplicationDetailsDto = await _mediator.Send(new GetJobApplicationDefaultsByUserIdQuery() { Id = userId });
        return Ok(jobApplicationDetailsDto);
    }
}
