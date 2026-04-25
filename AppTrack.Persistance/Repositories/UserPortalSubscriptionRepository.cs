using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class UserPortalSubscriptionRepository : GenericRepository<UserPortalSubscription>, IUserPortalSubscriptionRepository
{
    public UserPortalSubscriptionRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<UserPortalSubscription>> GetActiveSubscriptionsWithPortalsAsync()
        => await _context.UserPortalSubscriptions
            .Include(s => s.ProjectPortal)
            .Where(s => s.IsActive && s.ProjectPortal.IsActive)
            .ToListAsync();

    public async Task<List<UserPortalSubscription>> GetByUserIdAsync(string userId)
        => await _context.UserPortalSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync();

    public async Task<UserPortalSubscription?> GetByUserAndPortalAsync(string userId, int portalId)
        => await _context.UserPortalSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ProjectPortalId == portalId);

    public async Task UpsertAsync(string userId, int portalId, bool isActive)
    {
        var existing = await GetByUserAndPortalAsync(userId, portalId);
        if (existing is null)
        {
            await _context.UserPortalSubscriptions.AddAsync(
                new UserPortalSubscription { UserId = userId, ProjectPortalId = portalId, IsActive = isActive });
        }
        else
        {
            existing.IsActive = isActive;
        }
        await _context.SaveChangesAsync();
    }
}
