using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AiSettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AiSettingsController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        // GET api/<JobApplicationsController>/5
        [HttpGet("{userId}", Name = "GetAiSettingsForUser")]
        [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AiSettingsDto>> GetForUser(string userId)
        {
            var aiSettingsDto = await _mediator.Send(new GetAiSettingsByUserIdQuery() { UserId = userId });
            return Ok(aiSettingsDto);
        }
    }
}
