using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Api.IntegrationTests.SeedData.ProjectMonitoring;

internal static class ProjectMonitoringSettingsSeedsHelper
{
    public static async Task<int> CreateForTestUserAsync(
        IServiceProvider services, List<string> keywords, int intervalMinutes = 60)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppTrackDatabaseContext>();

        var settings = new ProjectMonitoringSettings
        {
            UserId = TestAuthHandler.TestUserId,
            Keywords = keywords,
            NotificationIntervalMinutes = intervalMinutes,
            NotificationEmail = "testuser@example.com"
        };
        db.ProjectMonitoringSettings.Add(settings);
        await db.SaveChangesAsync();
        return settings.Id;
    }
}
