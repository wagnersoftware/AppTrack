using AppTrack.Application.Models.Identity;

namespace AppTrack.Application.Contracts.Identity;

public interface IUserService
{
    Task<List<User>> GetUsers();

    Task<User?> GetUser(string userId);
}
