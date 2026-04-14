using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class AiSettingsRepository : GenericRepository<AiSettings>, IAiSettingsRepository
{
    public AiSettingsRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<AiSettings?> GetByIdWithPromptsAsync(int id)
    {
        return await _context.AiSettings
            .Include(s => s.PromptParameter)
            .Include(s => s.Prompts)
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task<AiSettings?> GetByUserIdWithPromptsReadOnlyAsync(string userId)
    {
        return await _context.AiSettings.AsNoTracking()
            .Include(s => s.PromptParameter)
            .Include(s => s.Prompts)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<AiSettings?> GetByUserIdWithPromptParameterAsync(string userId)
    {
        return await _context.AiSettings
            .Include(s => s.PromptParameter)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
