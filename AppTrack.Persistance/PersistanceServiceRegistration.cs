using AppTrack.Application.Contracts.Persistance;
using AppTrack.Persistance.DatabaseContext;
using AppTrack.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Persistance;

public static class PersistanceServiceRegistration
{
    public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppTrackDatabaseContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("AppTrackConnectionString"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IJobApplicationDefaultsRepository, JobApplicationDefaultsRepository>();
        services.AddScoped<IAiSettingsRepository, AiSettingsRepository>();
        services.AddScoped<IChatModelRepository, ChatModelRepository>();
        services.AddScoped<IBuiltInPromptRepository, BuiltInPromptRepository>();
        services.AddScoped<IFreelancerProfileRepository, FreelancerProfileRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
