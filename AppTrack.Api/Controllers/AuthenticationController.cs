using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Identity;
using AppTrack.Application.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthenticationController(IAuthService authService)
    {
        this._authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody]AuthRequest request)
    {
        var result = await _authService.Login(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> Register([FromBody]RegistrationRequest request)
    {
        var result = await _authService.Register(request);
        return Ok(result);
    }
}
