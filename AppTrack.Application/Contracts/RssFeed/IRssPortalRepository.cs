using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssPortalRepository : IGenericRepository<RssPortal>
{
    Task<List<RssPortal>> GetAllActiveAsync();
}
