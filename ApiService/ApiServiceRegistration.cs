using AppTrack.Frontend.ApiService.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppTrack.Frontend.ApiService;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServiceServices(this IServiceCollection services)
    {
        services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("https://localhost:7273"));
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
