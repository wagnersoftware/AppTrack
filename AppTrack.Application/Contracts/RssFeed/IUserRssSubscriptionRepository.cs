using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IUserRssSubscriptionRepository : IGenericRepository<UserRssSubscription>
{
    Task<List<UserRssSubscription>> GetActiveSubscriptionsWithPortalsAsync();
    Task<List<UserRssSubscription>> GetByUserIdAsync(string userId);
    Task<UserRssSubscription?> GetByUserAndPortalAsync(string userId, int portalId);
    Task UpsertAsync(string userId, int portalId, bool isActive);
}
