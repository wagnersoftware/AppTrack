using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<JobApplication>> GetAllForUserAsync(string userId)
    {
        return await _context.JobApplications.Where(x => x.UserId == userId).ToListAsync();
    }
}
