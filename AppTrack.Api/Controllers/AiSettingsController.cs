using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
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

        // GET api/<AiSettingsController>/5
        [HttpGet("{userId}", Name = "GetAiSettingsForUser")]
        [ProducesResponseType(typeof(AiSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AiSettingsDto>> GetForUser(string userId)
        {
            var aiSettingsDto = await _mediator.Send(new GetAiSettingsByUserIdQuery() { UserId = userId });
            return Ok(aiSettingsDto);
        }

        // PUT api/<AiSettingsController>/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Put([FromBody] UpdateAiSettingsCommand updateAiSettingsCommand)
        {
            await _mediator.Send(updateAiSettingsCommand);
            return NoContent();
        }
    }
}
