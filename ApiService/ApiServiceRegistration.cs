using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppTrack.Frontend.ApiService;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServiceServices(this IServiceCollection services)
    {
        services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("https://localhost:7273"));

        services.AddScoped<IJobApplicationService, JobApplicationService>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
