using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.RssFeed;

internal static class RssMonitoringSettingsSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, List<string> keywords, int intervalMinutes = 60)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var settings = new RssMonitoringSettings
        {
            UserId = TestAuthHandler.TestUserId,
            Keywords = keywords,
            PollIntervalMinutes = intervalMinutes,
            NotificationEmail = "testuser@example.com"
        };
        db.RssMonitoringSettings.Add(settings);
        await db.SaveChangesAsync();
        return settings.Id;
    }
}
