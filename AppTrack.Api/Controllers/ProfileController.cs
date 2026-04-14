using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Api.Mappings;
using AppTrack.Application.Features.FreelancerProfile.Commands.DeleteCv;
using AppTrack.Application.Features.FreelancerProfile.Commands.DeleteFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/profile")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/profile
    [HttpGet]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FreelancerProfileDto>> Get()
    {
        var dto = await _mediator.Send(new GetFreelancerProfileQuery());
        return Ok(dto);
    }

    // PUT api/profile
    [HttpPut]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FreelancerProfileDto>> Put([FromBody] UpsertFreelancerProfileCommand command)
    {
        var dto = await _mediator.Send(command);
        return Ok(dto);
    }

    // DELETE api/profile/cv
    [HttpDelete("cv")]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FreelancerProfileDto>> DeleteCv()
    {
        var dto = await _mediator.Send(new DeleteCvCommand());
        return Ok(dto);
    }

    // DELETE api/profile
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete()
    {
        await _mediator.Send(new DeleteFreelancerProfileCommand());
        return NoContent();
    }

    // POST api/profile/cv
    [HttpPost("cv")]
    [ProducesResponseType(typeof(FreelancerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FreelancerProfileDto>> UploadCv(IFormFile file)
    {
        var dto = await _mediator.Send(file.ToUploadCvCommand());
        return Ok(dto);
    }
}
