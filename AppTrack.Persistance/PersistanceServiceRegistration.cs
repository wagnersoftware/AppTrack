using AppTrack.Application.Contracts.Persistance;
using AppTrack.Persistance.DatabaseContext;
using AppTrack.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTrack.Persistance;

public static class PersistanceServiceRegistration
{
    public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration  configuration)
    {
        services.AddDbContext<AppTrackDatabaseContext>(options => {
            options.UseSqlServer(configuration.GetConnectionString("AppTrackConnectionString"));
            });
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

        return services;
    }
}
