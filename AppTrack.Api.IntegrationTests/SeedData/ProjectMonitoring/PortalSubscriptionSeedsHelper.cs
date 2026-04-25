using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.ProjectMonitoring;

internal static class PortalSubscriptionSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, int portalId, bool isActive = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var subscription = new UserPortalSubscription
        {
            UserId = TestAuthHandler.TestUserId,
            ProjectPortalId = portalId,
            IsActive = isActive
        };
        db.UserPortalSubscriptions.Add(subscription);
        await db.SaveChangesAsync();
        return subscription.Id;
    }

    public static async Task<int> GetFirstPortalIdAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();
        var portal = await db.ProjectPortals.FirstAsync(p => p.IsActive);
        return portal.Id;
    }
}
