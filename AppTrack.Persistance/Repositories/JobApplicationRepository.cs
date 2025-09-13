using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
{
    private readonly AppTrackDatabaseContext dbContext;

    public JobApplicationRepository(AppTrackDatabaseContext dbContext): base(dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task<bool> IsClientUnique(string client)
    {
        return await _context.JobApplications.Where(x => x.Client == client).AnyAsync();
    }
}
