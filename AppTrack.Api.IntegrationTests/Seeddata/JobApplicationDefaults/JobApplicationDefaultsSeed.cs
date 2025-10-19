using AppTrack.Api.IntegrationTests.SeedData.User;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Api.IntegrationTests.SeedData.JobApplicationDefaults;

internal static class JobApplicationDefaultsSeed
{
    internal static int JobApplicationDefaults1Id { get; private set; }
    internal static int JobApplicationDefaults2Id { get; private set; }

    internal static async Task AddJobApplicationDefaultsForUser1Async(AppTrackDatabaseContext dbContext)
    {
        var jobApplicationDefaults1 = new Domain.JobApplicationDefaults
        {
            UserId = ApplicationUserSeed.User1Id,
            Position = "Developer",
            Location = "Remote",
            Name = "Daniel",
        };

        await dbContext.JobApplicationDefaults.AddAsync(jobApplicationDefaults1);
        await dbContext.SaveChangesAsync();
        JobApplicationDefaults1Id = jobApplicationDefaults1.Id;
    }

    internal static async Task AddJobApplicationDefaultsForRandomUserAsync(AppTrackDatabaseContext dbContext)
    {
        var jobApplicationDefaults2 = new Domain.JobApplicationDefaults
        {
            UserId = Guid.NewGuid().ToString(),
            Position = "Developer",
            Location = "Remote",
            Name = "Daniel",
        };

        await dbContext.JobApplicationDefaults.AddAsync(jobApplicationDefaults2);
        await dbContext.SaveChangesAsync();
        JobApplicationDefaults2Id = jobApplicationDefaults2.Id;
    }
}