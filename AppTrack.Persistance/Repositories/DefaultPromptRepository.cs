using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Persistance.Repositories;

public class DefaultPromptRepository : GenericRepository<DefaultPrompt>, IDefaultPromptRepository
{
    public DefaultPromptRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }
}
