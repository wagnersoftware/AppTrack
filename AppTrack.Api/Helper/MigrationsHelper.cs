using Microsoft.EntityFrameworkCore;

namespace AppTrack.Api.Helper;

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
