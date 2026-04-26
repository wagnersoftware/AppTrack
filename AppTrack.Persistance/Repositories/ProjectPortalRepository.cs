using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProjectPortalRepository : GenericRepository<ProjectPortal>, IProjectPortalRepository
{
    public ProjectPortalRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<ProjectPortal>> GetAllActiveAsync()
        => await _context.ProjectPortals.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync();
}
