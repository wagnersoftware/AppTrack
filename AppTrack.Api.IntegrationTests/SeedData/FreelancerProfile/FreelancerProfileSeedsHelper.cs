using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.FreelancerProfile;

internal static class FreelancerProfileSeedsHelper
{
    public static async Task<int> CreateProfileForUserAsync(IServiceProvider services, string userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var profile = new Domain.FreelancerProfile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            HourlyRate = 100m,
            Skills = "C#, Azure",
        };

        await db.FreelancerProfiles.AddAsync(profile);
        await db.SaveChangesAsync();

        return profile.Id;
    }
}
