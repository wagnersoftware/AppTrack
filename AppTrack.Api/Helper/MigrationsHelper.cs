using Microsoft.EntityFrameworkCore;

namespace AppTrack.Api.Helper;

/// <summary>
/// Provides helper methods for applying database migrations during application startup.
/// </summary>
/// <remarks>This class is intended to be used in ASP.NET Core applications to ensure that required database
/// schema updates are applied automatically. All methods are static and should be invoked as part of the application's
/// initialization process.</remarks>
public static class MigrationsHelper
{
    public static async Task TryApplyDatabaseMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var identityDb = services.GetRequiredService<AppTrack.Identity.DBContext.AppTrackIdentityDbContext>();
            var mainDb = services.GetRequiredService<AppTrack.Persistance.DatabaseContext.AppTrackDatabaseContext>();

            await identityDb.Database.MigrateAsync();
            await mainDb.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration was not successful {ex.Message}");
            throw;
        }
    }
}
