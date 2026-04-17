using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class JobApplicationAiTextRepository : IJobApplicationAiTextRepository
{
    private readonly AppTrackDatabaseContext _context;

    public JobApplicationAiTextRepository(AppTrackDatabaseContext context)
    {
        _context = context;
    }

    public Task<JobApplicationAiText?> GetByIdAsync(int id) =>
        _context.JobApplicationAiTexts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<int> CountByJobApplicationAndPromptAsync(int jobApplicationId, string promptKey) =>
        _context.JobApplicationAiTexts
            .CountAsync(x => x.JobApplicationId == jobApplicationId && x.PromptKey == promptKey);

    public Task<JobApplicationAiText?> GetOldestByJobApplicationAndPromptAsync(int jobApplicationId, string promptKey) =>
        _context.JobApplicationAiTexts
            .Where(x => x.JobApplicationId == jobApplicationId && x.PromptKey == promptKey)
            .OrderBy(x => x.GeneratedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(JobApplicationAiText aiText)
    {
        await _context.JobApplicationAiTexts.AddAsync(aiText);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(JobApplicationAiText aiText)
    {
        _context.JobApplicationAiTexts.Remove(aiText);
        await _context.SaveChangesAsync();
    }
}
