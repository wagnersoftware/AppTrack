using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class RssPortalRepository : GenericRepository<RssPortal>, IRssPortalRepository
{
    public RssPortalRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<RssPortal>> GetAllActiveAsync()
        => await _context.RssPortals.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();
}
