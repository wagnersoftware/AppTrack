using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Persistance;

public static class PersistanceServiceRegistration
{
    public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration  configuration)
    {
        services.AddDbContext<AppTrackDatabaseContext>(options => {
            options.UseSqlServer(configuration.GetConnectionString("AppTrackConnectionString"));
            });
        return services;
    }
}
