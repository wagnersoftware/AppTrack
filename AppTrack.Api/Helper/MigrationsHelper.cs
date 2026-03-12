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
        var logger = services.GetRequiredService<ILogger>();

        try
        {
            var mainDb = services.GetRequiredService<AppTrack.Persistance.DatabaseContext.AppTrackDatabaseContext>();

            logger.LogInformation("Starting database-migrations");
            await mainDb.Database.MigrateAsync();
            logger.LogInformation("Databse migration successful");
        }
        catch (Exception ex)
        {
            // Log the exception as critical, but do not rethrow it to allow migration retries as configured in DB context options
            logger.LogCritical(ex, "Critical database migration exception {Message}", ex.Message);
        }
    }
}

