using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Persistance.Repositories;

public class BuiltInPromptRepository : GenericRepository<BuiltInPrompt>, IBuiltInPromptRepository
{
    public BuiltInPromptRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }
}
