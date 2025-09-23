using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class JobApplicationDefaultsRepository : GenericRepository<JobApplicationDefaults>, IJobApplicationDefaultsRepository
{
    public JobApplicationDefaultsRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<JobApplicationDefaults?> CreateForUserAsync(int userId)
    {
        var entityToCreate = new JobApplicationDefaults() { UserId = userId};
        await CreateAsync(entityToCreate);
        return entityToCreate;
    }

    public async Task<JobApplicationDefaults> GetByUserIdAsync(int userId)
    {
        return await _context.JobApplicationDefaults.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
    }
}
