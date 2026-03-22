using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class DefaultPromptRepository : GenericRepository<DefaultPrompt>, IDefaultPromptRepository
{
    public DefaultPromptRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<DefaultPrompt>> GetByLanguageAsync(string language)
    {
        return await _context.DefaultPrompts
            .AsNoTracking()
            .Where(p => p.Language == language)
            .ToListAsync();
    }
}
