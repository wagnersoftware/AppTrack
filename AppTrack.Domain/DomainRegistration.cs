using AppTrack.Domain.Contracts;
using AppTrack.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTrack.Domain;

public static class DomainRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPromptBuilder, PromptBuilder>();

        return services;
    }
}
