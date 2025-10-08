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

    public async Task<AiSettings?> GetByIdWithPromptParameterAsync(int id)
    {
        return await _context.AiSettings
            .Include(s => s.PromptParameter)
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task<AiSettings?> GetByUserIdWithPromptParameterAsync(string userId)
    {
        return await _context.AiSettings.AsNoTracking()
            .Include(s => s.PromptParameter)
            .SingleOrDefaultAsync(s => s.UserId == userId);

    }
}
