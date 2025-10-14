using AppTrack.Application.Contracts.Mediator;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppTrack.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            //register mediator handlers
            services.Scan(scan => scan
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.AddSingleton<IPromptBuilder, PromptBuilder>();

            return services;
        }

    }
}
