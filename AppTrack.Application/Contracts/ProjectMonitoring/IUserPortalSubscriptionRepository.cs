using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IUserPortalSubscriptionRepository : IGenericRepository<UserPortalSubscription>
{
    Task<List<UserPortalSubscription>> GetActiveSubscriptionsWithPortalsAsync();
    Task<List<UserPortalSubscription>> GetByUserIdAsync(string userId);
    Task<UserPortalSubscription?> GetByUserAndPortalAsync(string userId, int portalId);
    Task UpsertAsync(string userId, int portalId, bool isActive);
}
