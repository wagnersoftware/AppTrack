using AppTrack.Application.Contracts.Identity;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Models.Identity;
using AppTrack.Identity.Models;
using AppTrack.Identity.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppTrack.Identity.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, SignInManager<ApplicationUser> signInManager)
    {
        this._userManager = userManager;
        this._jwtSettings = jwtSettings.Value;
        this._signInManager = signInManager;
    }
    public async Task<AuthResponse> Login(AuthRequest request)
    {
        var validator = new AuthRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid request", validationResult);
        }

        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user == null)
        {
            throw new NotFoundException("User", request.UserName);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (result.Succeeded == false)
        {
            throw new BadRequestException($"Wrong credentials for user {request.UserName}");
        }

        var token = await GenerateToken(user);

        var authResponse = new AuthResponse
        {
            Id = user.Id,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserName = user.UserName!
        };

        return authResponse;
    }

    public async Task<RegistrationResponse> Register(RegistrationRequest request)
    {
        var validator = new RegistrationRequestValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid request", validationResult);
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded == false)
        {
            if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                throw new ConflictException("User with this username or email already exists.");
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var error in result.Errors)
            {
                stringBuilder.Append($"{error.Description}" + Environment.NewLine);
            }
            throw new BadRequestException($"User registration failed: {stringBuilder}");
        }

        await _userManager.AddToRoleAsync(user, "User");
        return new RegistrationResponse() { UserId = user.Id };
    }

    private async Task<JwtSecurityToken> GenerateToken(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        var roleClaims = userRoles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredeantials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: signingCredeantials);

        return jwtSecurityToken;
    }
}
