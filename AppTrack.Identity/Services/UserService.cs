using AppTrack.Application.Contracts.Identity;
using AppTrack.Application.Models.Identity;
using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace AppTrack.Identity.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        this._userManager = userManager;
    }
    public async Task<User?> GetUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if(user ==null)
        { 
            return null;
        }

        return new User()
        {
            Id = user.Id,
        };
    }

    public async Task<List<User>> GetUsers()
    {
        var users = await _userManager.GetUsersInRoleAsync("User");
        return users.Select(u => new User()
        {
            Id = u.Id,
        }).ToList();
    }
}
