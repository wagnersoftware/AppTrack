using AppTrack.Domain.Enums;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.JobApplication;

/// <summary>
/// Provides helper methods for seeding job application data for a specified user in the application database.
/// </summary>
/// <remarks>This static class is intended for use in scenarios such as testing or initial data setup, where
/// creating default job application records is required. All methods are asynchronous and require a valid service
/// provider with access to the application's database context.</remarks>
public static class JobApplicationSeedsHelper
{
    public static async Task<int> CreateJobApplicationForUserAsync(IServiceProvider services, string userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var jobApplication = new Domain.JobApplication
        {
            UserId = userId,
            Name = "Default Name",
            Position = "Default Position",
            Location = "Default Location",
            URL = "https://default.url",
            ApplicationText = "Default application text",
            Status = JobApplicationStatus.New,
            JobDescription = "Default job description",
            ContactPerson = "Default Contact Person",
            DurationInMonths = "12",
            StartDate = DateTime.UtcNow
        };

        await db.JobApplications.AddAsync(jobApplication);
        await db.SaveChangesAsync();

        return jobApplication.Id;
    }
}
