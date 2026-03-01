using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;

namespace AppTrack.Persistance.Repositories;

public class ChatModelRepository : GenericRepository<ChatModel>, IChatModelRepository
{
    public ChatModelRepository(AppTrackDatabaseContext dbContext) : base(dbContext)
    {
    }
}
