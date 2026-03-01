using AppTrack.Frontend.ApiService.ApiAuthenticationProvider;
using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppTrack.Frontend.ApiService;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiServiceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(configuration), message: "The base URL is not configured!");


        services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri(baseUrl));

        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IJobApplicationDefaultsService, JobApplicationDefaultsService>();
        services.AddScoped<IAiSettingsService, AiSettingsService>();
        services.AddScoped<IApplicationTextService, ApplicationTextService>();
        services.AddScoped<IChatModelsService, ChatModelsService>();

        services.AddSingleton<ApiAuthenticationStateProvider>();
        services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
