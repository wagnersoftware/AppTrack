using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Identity;
using AppTrack.Application.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTrack.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        this._authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        return Ok(await _authService.Login(request));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(CustomProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        return Ok(await _authService.Register(request));
    }
}
