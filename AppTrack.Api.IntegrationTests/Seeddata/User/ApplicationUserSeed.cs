using AppTrack.Identity.DBContext;
using AppTrack.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace AppTrack.Api.IntegrationTests.Seeddata.User
{
    internal static class ApplicationUserSeed
    {
        internal static string User1Id { get; private set; } = "00000000-0000-0000-0000-000000000001";
        internal static async Task AddUserAsync(AppTrackIdentityDbContext dbContext)
        {
            var user = new ApplicationUser
            {
                Id = User1Id,
                UserName = "testuser",
                EmailConfirmed = true,
            };

            var passwordHasher = new PasswordHasher<IdentityUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, "Test1234!");

            await dbContext.AddAsync(user);
        }
    }
}
