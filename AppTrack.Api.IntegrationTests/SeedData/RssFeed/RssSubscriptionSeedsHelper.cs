using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.RssFeed;

internal static class RssSubscriptionSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, int portalId, bool isActive = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var subscription = new UserRssSubscription
        {
            UserId = TestAuthHandler.TestUserId,
            RssPortalId = portalId,
            IsActive = isActive
        };
        db.UserRssSubscriptions.Add(subscription);
        await db.SaveChangesAsync();
        return subscription.Id;
    }

    public static async Task<int> GetFirstPortalIdAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();
        var portal = db.RssPortals.First(p => p.IsActive);
        return portal.Id;
    }
}
