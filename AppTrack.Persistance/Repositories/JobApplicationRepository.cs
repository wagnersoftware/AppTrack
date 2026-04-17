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

    public async Task<List<JobApplication>> GetAllForUserWithAiTextHistoryAsync(string userId)
    {
        return await _context.JobApplications
            .Where(x => x.UserId == userId)
            .Include(x => x.AiTextHistory.OrderByDescending(a => a.GeneratedAt))
            .ToListAsync();
    }

    public async Task<JobApplication?> GetByIdWithAiTextHistoryAsync(int id)
    {
        return await _context.JobApplications
            .Include(x => x.AiTextHistory.OrderByDescending(a => a.GeneratedAt))
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
