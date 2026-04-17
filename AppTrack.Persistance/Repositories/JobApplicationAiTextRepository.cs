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

    public Task<int> CountByJobApplicationAndPromptAsync(int jobApplicationId, string promptName) =>
        _context.JobApplicationAiTexts
            .CountAsync(x => x.JobApplicationId == jobApplicationId && x.PromptName == promptName);

    public Task<List<JobApplicationAiText>> GetOldestByJobApplicationAndPromptAsync(int jobApplicationId, string promptName, int keepNewest) =>
        _context.JobApplicationAiTexts
            .Where(x => x.JobApplicationId == jobApplicationId && x.PromptName == promptName)
            .OrderByDescending(x => x.GeneratedAt)
            .Skip(keepNewest)
            .ToListAsync();

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
