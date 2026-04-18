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

    public static async Task<int> CreateProfileWithCvForUserAsync(IServiceProvider services, string userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var profile = new Domain.FreelancerProfile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            CvBlobPath = "test-user/cv.pdf",
            CvFileName = "cv.pdf",
            CvText = "Sample CV text",
            CvUploadDate = DateTime.UtcNow,
        };

        await db.FreelancerProfiles.AddAsync(profile);
        await db.SaveChangesAsync();

        return profile.Id;
    }
}
