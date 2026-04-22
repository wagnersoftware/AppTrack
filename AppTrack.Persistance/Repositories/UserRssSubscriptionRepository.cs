using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class UserRssSubscriptionRepository : GenericRepository<UserRssSubscription>, IUserRssSubscriptionRepository
{
    public UserRssSubscriptionRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<UserRssSubscription>> GetActiveSubscriptionsWithPortalsAsync()
        => await _context.UserRssSubscriptions
            .AsNoTracking()
            .Include(s => s.RssPortal)
            .Where(s => s.IsActive && s.RssPortal.IsActive)
            .ToListAsync();

    public async Task<List<UserRssSubscription>> GetByUserIdAsync(string userId)
        => await _context.UserRssSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync();

    public async Task<UserRssSubscription?> GetByUserAndPortalAsync(string userId, int portalId)
        => await _context.UserRssSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.RssPortalId == portalId);

    public async Task UpsertAsync(string userId, int portalId, bool isActive)
    {
        var existing = await GetByUserAndPortalAsync(userId, portalId);
        if (existing is null)
        {
            await _context.UserRssSubscriptions.AddAsync(
                new UserRssSubscription { UserId = userId, RssPortalId = portalId, IsActive = isActive });
        }
        else
        {
            existing.IsActive = isActive;
        }
        await _context.SaveChangesAsync();
    }
}
