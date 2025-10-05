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

    public async Task<AiSettings> GetByUserIdAsync(string userId)
    {
        return await _context.AiSettings.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
    }
}
